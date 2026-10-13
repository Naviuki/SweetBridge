using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SweetWave.Web.Domain.Entities;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Infrastructure.Persistence;
using SweetWave.Web.Options;
using SweetWave.Web.Services.Catalog;
using SweetWave.Web.Services.Delivery;
using SweetWave.Web.Services.Seasons;

namespace SweetWave.Web.Services.Waves;

/// <summary>
/// Wave calculations and administration on top of EF Core and the logistics configuration.
/// </summary>
public sealed class WaveService : IWaveService
{
    private const int GramsPerKilogram = 1000;

    private readonly AppDbContext _dbContext;
    private readonly IDeliveryCostCalculator _deliveryCostCalculator;
    private readonly ISeasonPolicy _seasonPolicy;
    private readonly LogisticsOptions _options;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaveService"/> class.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <param name="deliveryCostCalculator">Delivery cost calculator.</param>
    /// <param name="seasonPolicy">Seasonal rules.</param>
    /// <param name="options">Logistics parameters.</param>
    /// <param name="timeProvider">Time provider.</param>
    public WaveService(
        AppDbContext dbContext,
        IDeliveryCostCalculator deliveryCostCalculator,
        ISeasonPolicy seasonPolicy,
        IOptions<LogisticsOptions> options,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(deliveryCostCalculator);
        ArgumentNullException.ThrowIfNull(seasonPolicy);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dbContext = dbContext;
        _deliveryCostCalculator = deliveryCostCalculator;
        _seasonPolicy = seasonPolicy;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WaveOffer>> GetOpenOffersAsync(
        ShippingDirection direction,
        IReadOnlyList<ProductSummary> products,
        int orderWeightGrams,
        bool thermoPackRequested,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(products);

        DateOnly today = Today;

        List<Wave> waves = await _dbContext.Waves
            .AsNoTracking()
            .Where(wave => wave.Direction == direction
                && wave.Status == WaveStatus.Planned
                && wave.OrderCutoffDate >= today)
            .OrderBy(wave => wave.ShipDate)
            .ToListAsync(cancellationToken);

        if (waves.Count == 0)
        {
            return [];
        }

        Dictionary<int, int> loadedGrams = await WaveLoadQueries.GetLoadedGramsAsync(
            _dbContext,
            [.. waves.Select(wave => wave.Id)],
            cancellationToken);

        return [.. waves.Select(wave => CreateOffer(wave, products, orderWeightGrams, thermoPackRequested, loadedGrams))];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WaveSummary>> GetUpcomingAsync(
        ShippingDirection direction,
        int count,
        CancellationToken cancellationToken)
    {
        DateOnly today = Today;

        List<Wave> waves = await _dbContext.Waves
            .AsNoTracking()
            .Where(wave => wave.Direction == direction
                && wave.Status == WaveStatus.Planned
                && wave.OrderCutoffDate >= today)
            .OrderBy(wave => wave.ShipDate)
            .Take(count)
            .ToListAsync(cancellationToken);

        Dictionary<int, int> loadedGrams = await WaveLoadQueries.GetLoadedGramsAsync(
            _dbContext,
            [.. waves.Select(wave => wave.Id)],
            cancellationToken);

        return [.. waves.Select(wave => CreateSummary(wave, loadedGrams.GetValueOrDefault(wave.Id)))];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WaveSummary>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<Wave> waves = await _dbContext.Waves
            .AsNoTracking()
            .OrderBy(wave => wave.ShipDate)
            .ThenBy(wave => wave.Direction)
            .ToListAsync(cancellationToken);

        Dictionary<int, int> loadedGrams = await WaveLoadQueries.GetLoadedGramsAsync(
            _dbContext,
            [.. waves.Select(wave => wave.Id)],
            cancellationToken);

        return [.. waves.Select(wave => CreateSummary(wave, loadedGrams.GetValueOrDefault(wave.Id)))];
    }

    /// <inheritdoc />
    public async Task<WaveSummary> CreateAsync(CreateWaveCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        Wave wave = Wave.Create(
            command.Direction,
            command.ShipDate,
            command.ShipDate.AddDays(-_options.OrderCutoffDaysBeforeShipment),
            _options.MaxWaveLoadKg * GramsPerKilogram,
            _options.MinWaveLoadKg * GramsPerKilogram);

        _dbContext.Waves.Add(wave);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateSummary(wave, 0);
    }

    /// <inheritdoc />
    public async Task DispatchAsync(int waveId, CancellationToken cancellationToken)
    {
        Wave wave = await _dbContext.Waves.FirstOrDefaultAsync(item => item.Id == waveId, cancellationToken)
            ?? throw new DomainException($"Волна {waveId} не найдена.");

        int loadedGrams = await WaveLoadQueries.GetLoadedGramsAsync(_dbContext, waveId, cancellationToken);
        wave.MarkDispatched(loadedGrams);

        DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

        List<Order> orders = await _dbContext.Orders
            .Where(order => order.WaveId == waveId
                && (order.Status == OrderStatus.Accepted || order.Status == OrderStatus.Assembling))
            .ToListAsync(cancellationToken);

        foreach (Order order in orders)
        {
            order.Ship(nowUtc);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private DateOnly Today => DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

    private WaveOffer CreateOffer(
        Wave wave,
        IReadOnlyList<ProductSummary> products,
        int orderWeightGrams,
        bool thermoPackRequested,
        IReadOnlyDictionary<int, int> loadedGrams)
    {
        int loaded = loadedGrams.GetValueOrDefault(wave.Id);
        bool thermoPackRequired = _seasonPolicy.IsThermoPackRequired(products, wave.ShipDate);
        bool thermoPackPaid = thermoPackRequired || thermoPackRequested;

        decimal fillPercentAfter = wave.GetFillPercent(loaded + orderWeightGrams);

        DeliveryCostResult cost = _deliveryCostCalculator.Calculate(
            new DeliveryCostRequest(orderWeightGrams, fillPercentAfter, thermoPackPaid));

        int extraDays = _seasonPolicy.GetExtraDeliveryDays(wave.ShipDate);

        return new WaveOffer(
            wave.Id,
            wave.Direction,
            wave.ShipDate,
            wave.OrderCutoffDate,
            wave.CapacityGrams,
            loaded,
            wave.GetFillPercent(loaded),
            fillPercentAfter,
            wave.CanAccept(loaded, orderWeightGrams),
            cost,
            thermoPackRequired,
            _options.MinDeliveryDays + extraDays,
            _options.MaxDeliveryDays + extraDays,
            wave.ShipDate.AddDays(_options.MinDeliveryDays + extraDays),
            wave.ShipDate.AddDays(_options.MaxDeliveryDays + extraDays));
    }

    private static WaveSummary CreateSummary(Wave wave, int loadedGrams) =>
        new(
            wave.Id,
            wave.Direction,
            wave.ShipDate,
            wave.OrderCutoffDate,
            wave.CapacityGrams,
            wave.MinLoadGrams,
            loadedGrams,
            wave.GetFillPercent(loadedGrams),
            wave.Status,
            wave.IsReadyToDispatch(loadedGrams));
}