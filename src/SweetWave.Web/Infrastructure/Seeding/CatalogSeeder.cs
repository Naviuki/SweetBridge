using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SweetWave.Web.Domain.Entities;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Infrastructure.Persistence;

namespace SweetWave.Web.Infrastructure.Seeding;

/// <summary>
/// Imports the catalogue from the CSV file shipped with the application.
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
    /// Initializes a new instance of the <see cref="CatalogSeeder"/> class.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    public CatalogSeeder(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds all products of the CSV file that are missing in the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of imported products.</returns>
    public async Task<int> SeedAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> products = ReadCatalog();

        HashSet<string> existingIds = (await _dbContext.Products
                .Select(product => product.Id)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);

        List<Product> newProducts = products.Where(product => !existingIds.Contains(product.Id)).ToList();

        if (newProducts.Count > 0)
        {
            _dbContext.Products.AddRange(newProducts);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return newProducts.Count;
    }

    private static List<Product> ReadCatalog()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, DataFolderName, CatalogFileName);

        if (!File.Exists(filePath))
        {
            throw new DomainException($"Файл каталога не найден: {filePath}.");
        }

        CsvConfiguration configuration = new(CultureInfo.InvariantCulture);

        using CsvReader reader = new(new StreamReader(filePath, Encoding.UTF8), configuration);

        return reader.GetRecords<CatalogCsvRecord>()
            .Select(ToProduct)
            .ToList();
    }

    private static Product ToProduct(CatalogCsvRecord record) => Product.Create(
        record.Id.Trim(),
        ParseRegion(record),
        record.Manufacturer,
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