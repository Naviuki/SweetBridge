using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Options;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Common;
using SweetBridge.Web.Services.Delivery;
using SweetBridge.Web.Services.Notifications;
using SweetBridge.Web.Services.Seasons;

namespace SweetBridge.Web.Services.Waves;

/// <summary>
/// Расчёты и администрирование волн поверх EF Core и логистической конфигурации.
/// </summary>
public sealed class WaveService : IWaveService
{
    private const int GramsPerKilogram = 1000;
    private const int ConcurrencyRetryCount = 3;

    private readonly AppDbContext _dbContext;
    private readonly IDeliveryCostCalculator _deliveryCostCalculator;
    private readonly ISeasonPolicy _seasonPolicy;
    private readonly INotificationService _notificationService;
    private readonly LogisticsOptions _options;
    private readonly IBusinessClock _clock;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="WaveService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="deliveryCostCalculator">Калькулятор стоимости доставки.</param>
    /// <param name="seasonPolicy">Сезонные правила.</param>
    /// <param name="notificationService">Сервис уведомлений.</param>
    /// <param name="options">Логистические параметры.</param>
    /// <param name="clock">Источник текущего времени.</param>
    public WaveService(
        AppDbContext dbContext,
        IDeliveryCostCalculator deliveryCostCalculator,
        ISeasonPolicy seasonPolicy,
        INotificationService notificationService,
        IOptions<LogisticsOptions> options,
        IBusinessClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(deliveryCostCalculator);
        ArgumentNullException.ThrowIfNull(seasonPolicy);
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clock);

        _dbContext = dbContext;
        _deliveryCostCalculator = deliveryCostCalculator;
        _seasonPolicy = seasonPolicy;
        _notificationService = notificationService;
        _options = options.Value;
        _clock = clock;
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

        List<Wave> waves = await GetOpenWavesAsync(direction, cancellationToken);

        return [.. waves.Select(wave => CreateOffer(wave, products, orderWeightGrams, thermoPackRequested))];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WaveSummary>> GetUpcomingAsync(
        ShippingDirection direction,
        int count,
        CancellationToken cancellationToken)
    {
        List<Wave> waves = await GetOpenWavesAsync(direction, cancellationToken);

        return [.. waves.Take(count).Select(CreateSummary)];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WaveSummary>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<Wave> waves = await _dbContext.Waves
            .AsNoTracking()
            .OrderBy(wave => wave.ShipDate)
            .ThenBy(wave => wave.Direction)
            .ToListAsync(cancellationToken);

        return [.. waves.Select(CreateSummary)];
    }

    /// <inheritdoc />
    public async Task<WaveSummary> CreateAsync(CreateWaveCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        int nextNumber = await _dbContext.Waves
            .AsNoTracking()
            .Select(wave => (int?)wave.Number)
            .MaxAsync(cancellationToken) ?? 0;

        Wave wave = Wave.Create(
            nextNumber + 1,
            command.Direction,
            command.ShipDate,
            command.ShipDate.AddDays(-_options.OrderCutoffDaysBeforeShipment),
            _options.MaxWaveLoadKg * GramsPerKilogram,
            _options.MinWaveLoadKg * GramsPerKilogram);

        _dbContext.Waves.Add(wave);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateSummary(wave);
    }

    /// <inheritdoc />
    public async Task DispatchAsync(int waveId, CancellationToken cancellationToken)
    {
        DateTime nowUtc = _clock.UtcNow;

        for (int attempt = 1; attempt <= ConcurrencyRetryCount; attempt++)
        {
            Wave wave = await _dbContext.Waves.FirstOrDefaultAsync(item => item.Id == waveId, cancellationToken)
                ?? throw new DomainException($"Волна {waveId} не найдена.");

            wave.MarkDispatched();

            List<Order> orders = await _dbContext.Orders
                .Where(order => order.WaveId == waveId
                    && (order.Status == OrderStatus.Accepted || order.Status == OrderStatus.Assembling))
                .ToListAsync(cancellationToken);

            foreach (Order order in orders)
            {
                order.Ship(nowUtc);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException) when (attempt < ConcurrencyRetryCount)
            {
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();

                continue;
            }

            foreach (Order order in orders.Where(order => !string.IsNullOrEmpty(order.UserId)))
            {
                await _notificationService.AddAsync(
                    order.UserId!,
                    $"Заказ {order.Number} отправлен",
                    $"Волна №{wave.Number} отправлена {wave.ShipDate:dd.MM.yyyy}. Ожидайте доставку в пункт выдачи.",
                    order.Id,
                    cancellationToken);
            }

            return;
        }

        throw new DomainException("Волна занята другим оператором. Попробуйте отправить её ещё раз.");
    }

    private async Task<List<Wave>> GetOpenWavesAsync(ShippingDirection direction, CancellationToken cancellationToken)
    {
        DateOnly today = _clock.LocalToday;

        return await _dbContext.Waves
            .AsNoTracking()
            .Where(wave => wave.Direction == direction
                && wave.Status == WaveStatus.Planned
                && wave.OrderCutoffDate >= today)
            .OrderBy(wave => wave.ShipDate)
            .ToListAsync(cancellationToken);
    }

    private WaveOffer CreateOffer(
        Wave wave,
        IReadOnlyList<ProductSummary> products,
        int orderWeightGrams,
        bool thermoPackRequested)
    {
        bool thermoPackRequired = _seasonPolicy.IsThermoPackRequired(products, wave.ShipDate);
        bool thermoPackPaid = thermoPackRequired || thermoPackRequested;

        decimal fillPercentAfter = wave.GetFillPercent(orderWeightGrams);

        DeliveryCostResult cost = _deliveryCostCalculator.Calculate(
            new DeliveryCostRequest(orderWeightGrams, fillPercentAfter, thermoPackPaid));

        int extraDays = _seasonPolicy.GetExtraDeliveryDays(wave.ShipDate);

        return new WaveOffer(
            wave.Id,
            wave.Number,
            wave.Direction,
            wave.ShipDate,
            wave.OrderCutoffDate,
            wave.CapacityGrams,
            wave.LoadedGrams,
            wave.GetFillPercent(0),
            fillPercentAfter,
            wave.CanAccept(orderWeightGrams),
            cost,
            thermoPackRequired,
            _options.MinDeliveryDays + extraDays,
            _options.MaxDeliveryDays + extraDays,
            wave.ShipDate.AddDays(_options.MinDeliveryDays + extraDays),
            wave.ShipDate.AddDays(_options.MaxDeliveryDays + extraDays));
    }

    private static WaveSummary CreateSummary(Wave wave) =>
        new(
            wave.Id,
            wave.Number,
            wave.Direction,
            wave.ShipDate,
            wave.OrderCutoffDate,
            wave.CapacityGrams,
            wave.MinLoadGrams,
            wave.LoadedGrams,
            wave.GetFillPercent(0),
            wave.Status,
            wave.IsReadyToDispatch());
}