using Microsoft.EntityFrameworkCore;
using SweetWave.Web.Infrastructure.Persistence;

namespace SweetWave.Web.Infrastructure.Seeding;

/// <summary>
/// Applies migrations and fills the database with the initial catalogue and waves on startup.
/// </summary>
public sealed partial class DatabaseInitializer
{
    private readonly AppDbContext _dbContext;
    private readonly CatalogSeeder _catalogSeeder;
    private readonly WaveSeeder _waveSeeder;
    private readonly ILogger<DatabaseInitializer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseInitializer"/> class.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <param name="catalogSeeder">Catalogue seeder.</param>
    /// <param name="waveSeeder">Wave seeder.</param>
    /// <param name="logger">Logger.</param>
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
    /// Creates the database if needed and seeds empty tables.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the initialization.</returns>
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