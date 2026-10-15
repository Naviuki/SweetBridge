using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Infrastructure.Persistence;

namespace SweetBridge.Web.Infrastructure.Seeding;

/// <summary>
/// Импортирует каталог из CSV-файла, поставляемого с приложением.
/// </summary>
public sealed class CatalogSeeder
{
    private const string CatalogFileName = "catalog.csv";
    private const string DataFolderName = "Data";
    private const string YakutiaRegionValue = "Якутия";
    private const string DonbassRegionValue = "ДНР";
    private const string YesValue = "да";
    private const string NoValue = "нет";

    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="CatalogSeeder"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public CatalogSeeder(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    /// <summary>
    /// Добавляет в базу все товары из CSV-файла, которых ещё нет.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество импортированных товаров.</returns>
    public async Task<int> SeedAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<CatalogCsvRecord> records = ReadCatalog();

        HashSet<string> existingIds = (await _dbContext.Products
                .Select(product => product.Id)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);

        // Уже загруженные товары не пересоздаём, но освежаем фотографию:
        // сидер запускается при каждом старте, поэтому новая картинка подхватывается сама.
        List<Product> stored = await _dbContext.Products.ToListAsync(cancellationToken);

        var imported = 0;
        var refreshed = 0;

        foreach (CatalogCsvRecord record in records)
        {
            string id = record.Id.Trim().ToUpperInvariant();

            if (existingIds.Contains(id))
            {
                Product? product = stored.FirstOrDefault(item =>
                    string.Equals(item.Id, id, StringComparison.Ordinal));

                string photo = record.Photo?.Trim() ?? string.Empty;

                if (product is not null
                    && !string.IsNullOrWhiteSpace(photo)
                    && !string.Equals(product.PhotoPath, photo, StringComparison.Ordinal))
                {
                    product.UpdatePhoto(photo);
                    refreshed++;
                }

                continue;
            }

            Producer producer = await GetOrCreateProducerAsync(record, cancellationToken);
            _dbContext.Products.Add(ToProduct(record, producer.Id));
            await _dbContext.SaveChangesAsync(cancellationToken);

            imported++;
        }

        if (refreshed > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return imported;
    }

    private async Task<Producer> GetOrCreateProducerAsync(CatalogCsvRecord record, CancellationToken cancellationToken)
    {
        Region region = ParseRegion(record);
        string name = record.Manufacturer.Trim();

        Producer? existing = await _dbContext.Producers
            .FirstOrDefaultAsync(producer => producer.Name == name && producer.Region == region, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        Producer created = Producer.Create(name, region, record.Description);
        _dbContext.Producers.Add(created);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return created;
    }

    private static List<CatalogCsvRecord> ReadCatalog()
    {
        string filePath = ResolveCatalogPath();

        CsvConfiguration configuration = new(CultureInfo.InvariantCulture);

        using CsvReader reader = new(new StreamReader(filePath, Encoding.UTF8), configuration);

        return reader.GetRecords<CatalogCsvRecord>().ToList();
    }

    /// <summary>
    /// Ищет CSV каталога: сначала в папке приложения, затем в выходной папке сборки.
    /// </summary>
    /// <returns>Путь к файлу каталога.</returns>
    private static string ResolveCatalogPath()
    {
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, DataFolderName, CatalogFileName),
            Path.Combine(Directory.GetCurrentDirectory(), DataFolderName, CatalogFileName)
        ];

        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return candidates[0];
    }

    private static Product ToProduct(CatalogCsvRecord record, int producerId) => Product.Create(
        record.Id.Trim(),
        producerId,
        ParseRegion(record),
        record.Name,
        record.Category,
        record.WeightGrams,
        record.Price,
        ParseTemperatureSensitive(record),
        record.ShelfLifeDays,
        string.IsNullOrWhiteSpace(record.Photo) ? null : record.Photo.Trim(),
        record.Description);

    private static Region ParseRegion(CatalogCsvRecord record) => record.Region.Trim() switch
    {
        YakutiaRegionValue => Region.Yakutia,
        DonbassRegionValue => Region.Donbass,
        var unknown => throw new DomainException($"Неизвестный регион «{unknown}» в строке каталога {record.Id}.")
    };

    private static bool ParseTemperatureSensitive(CatalogCsvRecord record) => record.TempSensitive.Trim() switch
    {
        YesValue => true,
        NoValue => false,
        var unknown => throw new DomainException(
            $"Неизвестный признак температурной чувствительности «{unknown}» в строке каталога {record.Id}.")
    };
}
