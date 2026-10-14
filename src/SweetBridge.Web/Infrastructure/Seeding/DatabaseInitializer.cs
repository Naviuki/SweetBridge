using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Infrastructure.Persistence;

namespace SweetBridge.Web.Infrastructure.Seeding;

/// <summary>
/// Применяет миграции и заполняет базу начальным каталогом и волнами при старте.
/// </summary>
public sealed partial class DatabaseInitializer
{
    private readonly AppDbContext _dbContext;
    private readonly CatalogSeeder _catalogSeeder;
    private readonly WaveSeeder _waveSeeder;
    private readonly ILogger<DatabaseInitializer> _logger;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="DatabaseInitializer"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="catalogSeeder">Сидер каталога.</param>
    /// <param name="waveSeeder">Сидер волн.</param>
    /// <param name="logger">Логгер.</param>
    public DatabaseInitializer(
        AppDbContext dbContext,
        CatalogSeeder catalogSeeder,
        WaveSeeder waveSeeder,
        ILogger<DatabaseInitializer> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(catalogSeeder);
        ArgumentNullException.ThrowIfNull(waveSeeder);
        ArgumentNullException.ThrowIfNull(logger);

        _dbContext = dbContext;
        _catalogSeeder = catalogSeeder;
        _waveSeeder = waveSeeder;
        _logger = logger;
    }

    /// <summary>
    /// Создаёт базу при необходимости и заполняет пустые таблицы.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение инициализации.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        int products = await _catalogSeeder.SeedAsync(cancellationToken);
        int waves = await _waveSeeder.SeedAsync(cancellationToken);

        if (products > 0 || waves > 0)
        {
            LogDatabaseSeeded(products, waves);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "База заполнена: товаров {ProductsCount}, волн {WavesCount}.")]
    private partial void LogDatabaseSeeded(int productsCount, int wavesCount);
}
