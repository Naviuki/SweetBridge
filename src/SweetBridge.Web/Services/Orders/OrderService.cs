using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Domain.ValueObjects;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Options;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Common;
using SweetBridge.Web.Services.Delivery;
using SweetBridge.Web.Services.Notifications;
using SweetBridge.Web.Services.Promos;
using SweetBridge.Web.Services.Seasons;

namespace SweetBridge.Web.Services.Orders;

/// <summary>
/// Сервис заказов: размещает заказы с пересчётом цен, веса и вместимости на сервере,
/// фиксирует оплату, находит заказы для отслеживания и переводит их по статусам.
/// </summary>
public sealed class OrderService : IOrderService
{
    private const int ConcurrencyRetryCount = 3;

    private readonly AppDbContext _dbContext;
    private readonly IDeliveryCostCalculator _deliveryCostCalculator;
    private readonly ISeasonPolicy _seasonPolicy;
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly IPromoService _promoService;
    private readonly INotificationService _notificationService;
    private readonly LogisticsOptions _options;
    private readonly IBusinessClock _clock;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="OrderService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="deliveryCostCalculator">Калькулятор стоимости доставки.</param>
    /// <param name="seasonPolicy">Сезонные правила.</param>
    /// <param name="orderNumberGenerator">Генератор номеров заказов.</param>
    /// <param name="promoService">Сервис промокодов.</param>
    /// <param name="notificationService">Сервис уведомлений.</param>
    /// <param name="options">Логистические параметры.</param>
    /// <param name="clock">Источник текущего времени.</param>
    public OrderService(
        AppDbContext dbContext,
        IDeliveryCostCalculator deliveryCostCalculator,
        ISeasonPolicy seasonPolicy,
        IOrderNumberGenerator orderNumberGenerator,
        IPromoService promoService,
        INotificationService notificationService,
        IOptions<LogisticsOptions> options,
        IBusinessClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(deliveryCostCalculator);
        ArgumentNullException.ThrowIfNull(seasonPolicy);
        ArgumentNullException.ThrowIfNull(orderNumberGenerator);
        ArgumentNullException.ThrowIfNull(promoService);
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clock);

        _dbContext = dbContext;
        _deliveryCostCalculator = deliveryCostCalculator;
        _seasonPolicy = seasonPolicy;
        _orderNumberGenerator = orderNumberGenerator;
        _promoService = promoService;
        _notificationService = notificationService;
        _options = options.Value;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<OrderSummary> PlaceAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Lines.Count == 0)
        {
            throw new DomainException("Корзина пуста: добавьте товары перед оформлением заказа.");
        }

        List<OrderItem> items = await BuildItemsAsync(command.Lines, cancellationToken);
        int orderWeightGrams = items.Sum(item => item.LineWeightGrams);

        decimal goodsCost = items.Sum(item => item.LineTotal);
        PromoApplyResult promo = await _promoService.ApplyAsync(command.PromoCode, goodsCost, cancellationToken);

        string? promoCode = promo.IsSuccess && promo.DiscountAmount > 0m ? command.PromoCode?.Trim().ToUpperInvariant() : null;
        decimal discountAmount = promoCode is null ? 0m : promo.DiscountAmount;

        for (int attempt = 1; attempt <= ConcurrencyRetryCount; attempt++)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            Wave wave = await _dbContext.Waves.FirstOrDefaultAsync(item => item.Id == command.WaveId, cancellationToken)
                ?? throw new DomainException("Выбранная волна не найдена. Обновите страницу и выберите волну заново.");

            if (!wave.IsOpenForOrders(_clock.LocalNow))
            {
                throw new DomainException("Приём заказов для этой волны завершён. Выберите другую волну.");
            }

            if (!wave.CanAccept(orderWeightGrams))
            {
                throw new DomainException(
                    $"Волна №{wave.Number} почти заполнена: свободно {wave.GetRemainingCapacity()} г, а заказ весит {orderWeightGrams} г.");
            }

            List<ProductSummary> products = await GetProductSummariesAsync(items, cancellationToken);
            bool thermoPackRequired = _seasonPolicy.IsThermoPackRequired(products, wave.ShipDate);

            DeliveryCostResult cost = _deliveryCostCalculator.Calculate(new DeliveryCostRequest(
                orderWeightGrams,
                wave.GetFillPercent(orderWeightGrams),
                thermoPackRequired || command.ThermoPackRequested));

            DateTime nowUtc = _clock.UtcNow;
            string number = await _orderNumberGenerator.GenerateAsync(cancellationToken);

            Order order = Order.Create(
                number,
                command.UserId,
                nowUtc,
                command.BuyerName,
                PhoneNumber.Parse(command.BuyerPhone).Value,
                command.IsGift,
                command.RecipientName,
                command.IsGift ? PhoneNumber.Parse(command.RecipientPhone).Value : null,
                command.RecipientCity ?? string.Empty,
                command.PickupPointId,
                command.RecipientPickupPoint,
                command.GiftMessage,
                wave.Id);

            foreach (OrderItem item in items)
            {
                order.AddItem(item);
            }

            order.ApplyCosts(
                cost.ShippingCost,
                cost.ThermoPackCost,
                cost.DiscountPercent,
                discountAmount,
                promoCode);

            order.SetPaymentDeadline(nowUtc.AddMinutes(_options.PaymentHoldMinutes));

            wave.Reserve(orderWeightGrams);

            _dbContext.Orders.Add(order);

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

            if (promoCode is not null)
            {
                await _promoService.RegisterUsageAsync(promoCode, cancellationToken);
            }

            if (!string.IsNullOrEmpty(order.UserId))
            {
                await _notificationService.AddAsync(
                    order.UserId,
                    $"Заказ {order.Number} принят",
                    $"Сумма к оплате: {order.Total:N0} ₽. Оплатите заказ до {order.PaymentDueAtUtc:dd.MM.yyyy HH:mm} UTC.",
                    order.Id,
                    cancellationToken);
            }

            return CreateSummary(order, wave);
        }

        throw new DomainException("Волна занята другим покупателем. Попробуйте оформить заказ ещё раз.");
    }

    /// <inheritdoc />
    public async Task<OrderSummary?> FindAsync(string number, string phone, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(number) || string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        Order? order = await LoadOrderAsync(number, cancellationToken);

        if (order is null)
        {
            return null;
        }

        string normalizedPhone = PhoneNumber.Parse(phone).Value;
        bool matches = order.BuyerPhone == normalizedPhone || order.RecipientPhone == normalizedPhone;

        return matches ? await CreateSummaryAsync(order, cancellationToken) : null;
    }

    /// <inheritdoc />
    public async Task<OrderSummary?> FindByNumberAsync(string number, CancellationToken cancellationToken)
    {
        Order? order = await LoadOrderAsync(number, cancellationToken);

        return order is null ? null : await CreateSummaryAsync(order, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrderSummary>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        List<Order> orders = await _dbContext.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(order => order.Items)
            .Include(order => order.History)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        Dictionary<int, Wave> waves = await GetWavesAsync([.. orders.Select(order => order.WaveId)], cancellationToken);

        return [.. orders.Select(order => CreateSummary(order, waves.GetValueOrDefault(order.WaveId)))];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrderSummary>> GetOrdersAsync(OrderFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<Order> orders = _dbContext.Orders
            .AsNoTracking()
            .AsSplitQuery()
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

        if (filter.Search is not null)
        {
            orders = orders.Where(order => order.Number.Contains(filter.Search));
        }

        List<Order> result = await orders
            .OrderByDescending(order => order.Id)
            .ToListAsync(cancellationToken);

        Dictionary<int, Wave> waves = await GetWavesAsync([.. result.Select(order => order.WaveId).Distinct()], cancellationToken);

        return [.. result.Select(order => CreateSummary(order, waves.GetValueOrDefault(order.WaveId)))];
    }

    /// <inheritdoc />
    public async Task<OrderSummary> AdvanceStatusAsync(int orderId, CancellationToken cancellationToken)
    {
        Order order = await GetRequiredOrderAsync(orderId, cancellationToken);
        Wave wave = await GetRequiredWaveAsync(order.WaveId, cancellationToken);

        order.AdvanceStatus(_clock.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await NotifyAboutStatusAsync(order, wave, cancellationToken);

        return CreateSummary(order, wave);
    }

    /// <inheritdoc />
    public async Task<OrderSummary> MarkPaidAsync(string orderNumber, CancellationToken cancellationToken)
    {
        Order order = await LoadOrderForUpdateAsync(orderNumber, cancellationToken)
            ?? throw new DomainException($"Заказ {orderNumber} не найден.");

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            return await CreateSummaryAsync(order, cancellationToken);
        }

        Wave wave = await GetRequiredWaveAsync(order.WaveId, cancellationToken);

        order.MarkPaid();
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(order.UserId))
        {
            await _notificationService.AddAsync(
                order.UserId,
                $"Оплата заказа {order.Number} получена",
                $"Заказ на сумму {order.Total:N0} ₽ оплачен и ждёт отправки в волне №{wave.Number} от {wave.ShipDate:dd.MM.yyyy}.",
                order.Id,
                cancellationToken);
        }

        return CreateSummary(order, wave);
    }

    /// <inheritdoc />
    public Task<OrderSummary> CancelByCustomerAsync(int orderId, CancellationToken cancellationToken) =>
        CancelAsync(orderId, byCustomer: true, cancellationToken);

    /// <inheritdoc />
    public Task<OrderSummary> CancelByAdminAsync(int orderId, CancellationToken cancellationToken) =>
        CancelAsync(orderId, byCustomer: false, cancellationToken);

    /// <inheritdoc />
    public async Task<int> ReleaseExpiredOrdersAsync(CancellationToken cancellationToken)
    {
        DateTime nowUtc = _clock.UtcNow;

        List<Order> expired = await _dbContext.Orders
            .Where(order => order.PaymentStatus == PaymentStatus.Pending
                && order.PaymentDueAtUtc != null
                && order.PaymentDueAtUtc < nowUtc
                && order.Status == OrderStatus.Accepted)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
        {
            return 0;
        }

        List<Wave> waves = await _dbContext.Waves
            .Where(wave => expired.Select(order => order.WaveId).Contains(wave.Id))
            .ToListAsync(cancellationToken);

        Dictionary<int, Wave> wavesById = waves.ToDictionary(wave => wave.Id);

        foreach (Order order in expired)
        {
            order.Cancel(nowUtc, byCustomer: false);

            if (wavesById.TryGetValue(order.WaveId, out Wave? wave) && wave.Status == WaveStatus.Planned)
            {
                wave.Release(order.TotalWeightGrams);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return expired.Count;
    }

    private async Task<OrderSummary> CancelAsync(int orderId, bool byCustomer, CancellationToken cancellationToken)
    {
        Order order = await GetRequiredOrderAsync(orderId, cancellationToken);
        Wave wave = await GetRequiredWaveAsync(order.WaveId, cancellationToken);

        order.Cancel(_clock.UtcNow, byCustomer);

        if (wave.Status == WaveStatus.Planned)
        {
            wave.Release(order.TotalWeightGrams);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(order.UserId))
        {
            await _notificationService.AddAsync(
                order.UserId,
                $"Заказ {order.Number} отменён",
                $"Заказ на сумму {order.Total:N0} ₽ отменён. Вес освобождён в волне №{wave.Number}.",
                order.Id,
                cancellationToken);
        }

        return CreateSummary(order, wave);
    }

    private async Task NotifyAboutStatusAsync(Order order, Wave wave, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(order.UserId))
        {
            return;
        }

        string title = order.Status switch
        {
            OrderStatus.Assembling => $"Заказ {order.Number} собирается",
            OrderStatus.InTransit => $"Заказ {order.Number} в пути",
            OrderStatus.ReadyForPickup => $"Заказ {order.Number} готов к выдаче",
            OrderStatus.Received => $"Заказ {order.Number} получен",
            OrderStatus.Cancelled => $"Заказ {order.Number} отменён",
            _ => $"Заказ {order.Number}: статус изменён"
        };

        string details = order.Status switch
        {
            OrderStatus.Assembling => $"Волна №{wave.Number} комплектуется, отправка {wave.ShipDate:dd.MM.yyyy}.",
            OrderStatus.InTransit => $"Волна №{wave.Number} отправлена {wave.ShipDate:dd.MM.yyyy}.",
            OrderStatus.ReadyForPickup => $"Заказ прибыл: {order.RecipientPickupPoint ?? order.RecipientCity}.",
            OrderStatus.Received => "Спасибо, что выбрали «Сладкий мост»!",
            OrderStatus.Cancelled => "Заказ отменён, вес возвращён в волну.",
            _ => "Статус заказа обновлён."
        };

        await _notificationService.AddAsync(order.UserId, title, details, order.Id, cancellationToken);
    }

    private Task<Order?> LoadOrderAsync(string number, CancellationToken cancellationToken)
    {
        string normalizedNumber = number.Trim().ToUpperInvariant();

        return _dbContext.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(order => order.Items)
            .Include(order => order.History)
            .FirstOrDefaultAsync(order => order.Number == normalizedNumber, cancellationToken);
    }

    private Task<Order?> LoadOrderForUpdateAsync(string number, CancellationToken cancellationToken)
    {
        string normalizedNumber = number.Trim().ToUpperInvariant();

        return _dbContext.Orders
            .AsSplitQuery()
            .Include(order => order.Items)
            .Include(order => order.History)
            .FirstOrDefaultAsync(order => order.Number == normalizedNumber, cancellationToken);
    }

    private async Task<OrderSummary> CreateSummaryAsync(Order order, CancellationToken cancellationToken)
    {
        Wave wave = await GetRequiredWaveAsync(order.WaveId, cancellationToken);

        return CreateSummary(order, wave);
    }

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

    private async Task<Order> GetRequiredOrderAsync(int orderId, CancellationToken cancellationToken) =>
        await _dbContext.Orders.FirstOrDefaultAsync(item => item.Id == orderId, cancellationToken)
        ?? throw new DomainException($"Заказ {orderId} не найден.");

    private async Task<Wave> GetRequiredWaveAsync(int waveId, CancellationToken cancellationToken) =>
        await _dbContext.Waves.AsNoTracking().FirstOrDefaultAsync(item => item.Id == waveId, cancellationToken)
        ?? throw new DomainException($"Волна {waveId} не найдена.");

    private async Task<Dictionary<int, Wave>> GetWavesAsync(IReadOnlyCollection<int> waveIds, CancellationToken cancellationToken)
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
            .ToDictionaryAsync(wave => wave.Id, cancellationToken);
    }

    private static OrderSummary CreateSummary(Order order, Wave? wave) =>
        new(
            order.Id,
            order.Number,
            order.UserId,
            order.CreatedAtUtc,
            order.Status,
            order.PaymentStatus,
            order.IsGift,
            order.BuyerName,
            order.BuyerPhone,
            order.RecipientName,
            order.RecipientPhone,
            order.RecipientCity,
            order.RecipientPickupPoint,
            order.GiftMessage,
            order.WaveId,
            wave?.Number ?? 0,
            wave?.ShipDate ?? DateOnly.MinValue,
            order.TotalWeightGrams,
            order.GoodsCost,
            order.DiscountAmount,
            order.PromoCode,
            order.ShippingCost,
            order.ThermoPackCost,
            order.FillDiscountPercent,
            order.Total,
            order.PaymentDueAtUtc,
            [.. order.Items.Select(item => new OrderLineSummary(
                item.ProductId,
                item.ProductName,
                item.UnitPrice,
                item.Quantity))],
            [.. order.History
                .OrderBy(entry => entry.ChangedAtUtc)
                .Select(entry => new OrderStatusPoint(entry.Status, entry.ChangedAtUtc))]);
}