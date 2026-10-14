using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Infrastructure.Persistence;

namespace SweetBridge.Web.Infrastructure.Seeding;

/// <summary>
/// Применяет миграции и заполняет базу начальным каталогом и волнами при старте.
/// </summary>
/// <param name="dbContext">Контекст базы данных.</param>
/// <param name="catalogSeeder">Сидер каталога.</param>
/// <param name="waveSeeder">Сидер волн.</param>
/// <param name="referenceDataSeeder">Сидер справочных данных.</param>
/// <param name="environment">Среда выполнения: определяет корень для файлов данных.</param>
/// <param name="logger">Журнал событий.</param>
public sealed partial class DatabaseInitializer(
    AppDbContext dbContext,
    CatalogSeeder catalogSeeder,
    WaveSeeder waveSeeder,
    ReferenceDataSeeder referenceDataSeeder,
    IHostEnvironment environment,
    ILogger<DatabaseInitializer> logger)
{
    /// <summary>
    /// Создаёт базу при необходимости и заполняет пустые таблицы.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение инициализации.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        EnsureDataDirectory();

        await dbContext.Database.MigrateAsync(cancellationToken);

        int products = await catalogSeeder.SeedAsync(cancellationToken);
        int waves = await waveSeeder.SeedAsync(cancellationToken);
        await referenceDataSeeder.SeedAsync(cancellationToken);

        if (products > 0 || waves > 0)
        {
            LogDatabaseSeeded(logger, products, waves);
        }
    }

    private void EnsureDataDirectory()
    {
        string? dataSource = dbContext.Database.GetDbConnection().DataSource;

        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
        {
            return;
        }

        string fullPath = Path.IsPathRooted(dataSource)
            ? dataSource
            : Path.Combine(environment.ContentRootPath, dataSource);

        string? directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "База заполнена: товаров {ProductsCount}, волн {WavesCount}.")]
    private static partial void LogDatabaseSeeded(ILogger logger, int productsCount, int wavesCount);
}