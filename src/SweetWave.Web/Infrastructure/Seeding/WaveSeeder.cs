using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SweetWave.Web.Domain.Entities;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Infrastructure.Persistence;
using SweetWave.Web.Options;

namespace SweetWave.Web.Infrastructure.Seeding;

/// <summary>
/// Creates the planned waves for both directions when the database has none.
/// </summary>
public sealed class WaveSeeder
{
    private const int WavesPerDirection = 6;
    private const int GramsPerKilogram = 1000;

    private readonly AppDbContext _dbContext;
    private readonly LogisticsOptions _options;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaveSeeder"/> class.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <param name="options">Logistics parameters.</param>
    /// <param name="timeProvider">Time provider.</param>
    public WaveSeeder(AppDbContext dbContext, IOptions<LogisticsOptions> options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dbContext = dbContext;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Adds the planned waves when the database has none.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of created waves.</returns>
    public async Task<int> SeedAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Waves.AnyAsync(cancellationToken))
        {
            return 0;
        }

        DateOnly firstShipDate = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime)
            .AddDays(_options.OrderCutoffDaysBeforeShipment + 1);

        List<Wave> waves = [];
        foreach (ShippingDirection direction in new[] { ShippingDirection.YakutiaToDonbass, ShippingDirection.DonbassToYakutia })
        {
            for (var waveIndex = 0; waveIndex < WavesPerDirection; waveIndex++)
            {
                DateOnly shipDate = firstShipDate.AddDays(waveIndex * _options.WaveIntervalDays);

                waves.Add(Wave.Create(
                    direction,
                    shipDate,
                    shipDate.AddDays(-_options.OrderCutoffDaysBeforeShipment),
                    _options.MaxWaveLoadKg * GramsPerKilogram,
                    _options.MinWaveLoadKg * GramsPerKilogram));
            }
        }

        _dbContext.Waves.AddRange(waves);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return waves.Count;
    }
}