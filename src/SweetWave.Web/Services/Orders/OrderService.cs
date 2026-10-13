using Microsoft.EntityFrameworkCore;
using SweetWave.Web.Domain.Entities;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Infrastructure.Persistence;
using SweetWave.Web.Services.Catalog;
using SweetWave.Web.Services.Delivery;
using SweetWave.Web.Services.Seasons;

namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Order service: places orders with server side recalculation of prices, weights and capacity,
/// finds orders for tracking and moves them through the status lifecycle.
/// </summary>
public sealed class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly IDeliveryCostCalculator _deliveryCostCalculator;
    private readonly ISeasonPolicy _seasonPolicy;
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderService"/> class.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <param name="deliveryCostCalculator">Delivery cost calculator.</param>
    /// <param name="seasonPolicy">Seasonal rules.</param>
    /// <param name="orderNumberGenerator">Order number generator.</param>
    /// <param name="timeProvider">Time provider.</param>
    public OrderService(
        AppDbContext dbContext,
        IDeliveryCostCalculator deliveryCostCalculator,
        ISeasonPolicy seasonPolicy,
        IOrderNumberGenerator orderNumberGenerator,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(deliveryCostCalculator);
        ArgumentNullException.ThrowIfNull(seasonPolicy);
        ArgumentNullException.ThrowIfNull(orderNumberGenerator);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dbContext = dbContext;
        _deliveryCostCalculator = deliveryCostCalculator;
        _seasonPolicy = seasonPolicy;
        _orderNumberGenerator = orderNumberGenerator;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<OrderSummary> PlaceAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Lines.Count == 0)
        {
            throw new DomainException("Корзина пуста: добавьте товары перед оформлением заказа.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        Wave wave = await _dbContext.Waves.FirstOrDefaultAsync(item => item.Id == command.WaveId, cancellationToken)
            ?? throw new DomainException("Выбранная волна не найдена. Обновите страницу и выберите волну заново.");

        if (!wave.IsOpenForOrders(Today))
        {
            throw new DomainException("Приём заказов для этой волны завершён. Выберите другую волну.");
        }

        List<OrderItem> items = await BuildItemsAsync(command.Lines, cancellationToken);
        int orderWeightGrams = items.Sum(item => item.LineWeightGrams);

        int loadedGrams = await WaveLoadQueries.GetLoadedGramsAsync(_dbContext, wave.Id, cancellationToken);

        if (!wave.CanAccept(loadedGrams, orderWeightGrams))
        {
            throw new DomainException(
                $"Волна {wave.ShipDate:dd.MM.yyyy} почти заполнена: свободно {wave.CapacityGrams - loadedGrams} г, а заказ весит {orderWeightGrams} г.");
        }

        List<ProductSummary> products = await GetProductSummariesAsync(items, cancellationToken);
        bool thermoPackRequired = _seasonPolicy.IsThermoPackRequired(products, wave.ShipDate);

        DeliveryCostResult cost = _deliveryCostCalculator.Calculate(new DeliveryCostRequest(
            orderWeightGrams,
            wave.GetFillPercent(loadedGrams + orderWeightGrams),
            thermoPackRequired || command.ThermoPackRequested));

        string number = await _orderNumberGenerator.GenerateAsync(cancellationToken);
        DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

        Order order = Order.Create(
            wave.Id,
            number,
            nowUtc,
            command.BuyerName,
            command.BuyerPhone,
            command.IsGift,
            command.RecipientName,
            command.RecipientPhone,
            command.RecipientCity,
            command.RecipientPickupPoint,
            command.GiftMessage);

        foreach (OrderItem item in items)
        {
            order.AddItem(item);
        }

        order.ApplyShippingCosts(cost.ShippingCost, cost.ThermoPackCost, cost.DiscountPercent);

        _dbContext.Orders.Add(order);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreateSummary(order, wave.ShipDate);
    }

    /// <inheritdoc />
    public async Task<OrderSummary?> FindAsync(string number, string phone, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(number) || string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        string normalizedNumber = number.Trim().ToUpperInvariant();
        string normalizedPhone = NormalizePhone(phone);

        Order? order = await _dbContext.Orders
            .AsNoTracking()
            .Include(item => item.Items)
            .Include(item => item.History)
            .FirstOrDefaultAsync(item => item.Number == normalizedNumber, cancellationToken);

        if (order is null || !HasMatchingPhone(order, normalizedPhone))
        {
            return null;
        }

        DateOnly waveShipDate = await GetWaveShipDateAsync(order.WaveId, cancellationToken);

        return CreateSummary(order, waveShipDate);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrderSummary>> GetOrdersAsync(OrderFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<Order> orders = _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .Include(order => order.History);

        if (filter.Status is not null)
        {
            orders = orders.Where(order => order.Status == filter.Status.Value);
        }

        if (filter.WaveId is not null)
        {
            orders = orders.Where(order => order.WaveId == filter.WaveId.Value);
        }

        List<Order> result = await orders
            .OrderByDescending(order => order.Id)
            .ToListAsync(cancellationToken);

        Dictionary<int, DateOnly> waveDates = await GetWaveDatesAsync(
            [.. result.Select(order => order.WaveId).Distinct()],
            cancellationToken);

        return
        [
            .. result.Select(order => CreateSummary(order, waveDates.GetValueOrDefault(order.WaveId)))
        ];
    }

    /// <inheritdoc />
    public async Task<OrderSummary> AdvanceStatusAsync(int orderId, CancellationToken cancellationToken)
    {
        Order order = await _dbContext.Orders.FirstOrDefaultAsync(item => item.Id == orderId, cancellationToken)
            ?? throw new DomainException($"Заказ {orderId} не найден.");

        order.AdvanceStatus(_timeProvider.GetUtcNow().UtcDateTime);

        await _dbContext.SaveChangesAsync(cancellationToken);

        DateOnly waveShipDate = await GetWaveShipDateAsync(order.WaveId, cancellationToken);

        return CreateSummary(order, waveShipDate);
    }

    private DateOnly Today => DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

    private async Task<List<OrderItem>> BuildItemsAsync(IReadOnlyList<OrderLineRequest> lines, CancellationToken cancellationToken)
    {
        string[] ids = [.. lines.Select(line => line.ProductId)];

        Dictionary<string, Product> products = await _dbContext.Products
            .Where(product => product.IsActive && ids.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, StringComparer.Ordinal, cancellationToken);

        List<OrderItem> items = [];
        foreach (OrderLineRequest line in lines)
        {
            if (!products.TryGetValue(line.ProductId, out Product? product))
            {
                throw new DomainException($"Товар {line.ProductId} больше не доступен в каталоге.");
            }

            items.Add(OrderItem.Create(product.Id, product.Name, product.Price, product.WeightGrams, line.Quantity));
        }

        return items;
    }

    private async Task<List<ProductSummary>> GetProductSummariesAsync(List<OrderItem> items, CancellationToken cancellationToken)
    {
        string[] ids = [.. items.Select(item => item.ProductId)];

        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => ids.Contains(product.Id))
            .Select(product => ProductSummary.FromProduct(product))
            .ToListAsync(cancellationToken);
    }

    private async Task<DateOnly> GetWaveShipDateAsync(int waveId, CancellationToken cancellationToken) =>
        (await GetWaveDatesAsync([waveId], cancellationToken)).GetValueOrDefault(waveId);

    private async Task<Dictionary<int, DateOnly>> GetWaveDatesAsync(IReadOnlyCollection<int> waveIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(waveIds);

        if (waveIds.Count == 0)
        {
            return [];
        }

        int[] ids = [.. waveIds];

        return await _dbContext.Waves
            .AsNoTracking()
            .Where(wave => ids.Contains(wave.Id))
            .ToDictionaryAsync(wave => wave.Id, wave => wave.ShipDate, cancellationToken);
    }

    private static bool HasMatchingPhone(Order order, string normalizedPhone) =>
        NormalizePhone(order.BuyerPhone) == normalizedPhone
        || NormalizePhone(order.RecipientPhone) == normalizedPhone;

    private static string NormalizePhone(string phone) => new(phone.Where(char.IsAsciiDigit).ToArray());

    private static OrderSummary CreateSummary(Order order, DateOnly waveShipDate) =>
        new(
            order.Id,
            order.Number,
            order.CreatedAtUtc,
            order.Status,
            order.IsGift,
            order.BuyerName,
            order.BuyerPhone,
            order.RecipientName,
            order.RecipientPhone,
            order.RecipientCity,
            order.RecipientPickupPoint,
            order.GiftMessage,
            order.WaveId,
            waveShipDate,
            order.TotalWeightGrams,
            order.GoodsCost,
            order.ShippingCost,
            order.ThermoPackCost,
            order.AppliedDiscountPercent,
            [.. order.Items.Select(item => new OrderLineSummary(item.ProductName, item.UnitPrice, item.Quantity))],
            [.. order.History
                .OrderBy(entry => entry.ChangedAtUtc)
                .Select(entry => new OrderStatusPoint(entry.Status, entry.ChangedAtUtc))]);
}