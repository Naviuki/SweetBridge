using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Options;
using SweetBridge.Web.Services.Common;

namespace SweetBridge.Web.Infrastructure.Seeding;

/// <summary>
/// Создаёт запланированные волны обоих направлений, если в базе их нет.
/// </summary>
public sealed class WaveSeeder
{
    private const int WavesPerDirection = 6;
    private const int GramsPerKilogram = 1000;

    private readonly AppDbContext _dbContext;
    private readonly LogisticsOptions _options;
    private readonly IBusinessClock _clock;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="WaveSeeder"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="options">Логистические параметры.</param>
    /// <param name="clock">Источник текущей даты.</param>
    public WaveSeeder(AppDbContext dbContext, IOptions<LogisticsOptions> options, IBusinessClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clock);

        _dbContext = dbContext;
        _options = options.Value;
        _clock = clock;
    }

    /// <summary>
    /// Добавляет запланированные волны, если в базе их нет.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество созданных волн.</returns>
    public async Task<int> SeedAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Waves.AnyAsync(cancellationToken))
        {
            return 0;
        }

        DateOnly firstShipDate = _clock.LocalToday.AddDays(_options.OrderCutoffDaysBeforeShipment + 1);

        var waves = new List<Wave>();
        var number = 1;

        foreach (ShippingDirection direction in new[] { ShippingDirection.YakutiaToDonbass, ShippingDirection.DonbassToYakutia })
        {
            for (var waveIndex = 0; waveIndex < WavesPerDirection; waveIndex++)
            {
                DateOnly shipDate = firstShipDate.AddDays(waveIndex * _options.WaveIntervalDays);

                waves.Add(Wave.Create(
                    number++,
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