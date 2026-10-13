using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;

namespace SweetWave.Web.Domain.Entities;

/// <summary>
/// A customer order placed into one of the waves.
/// </summary>
public sealed class Order
{
    private readonly List<OrderItem> _items = [];
    private readonly List<OrderStatusHistoryEntry> _history = [];

    private Order()
    {
    }

    private Order(
        int waveId,
        string number,
        DateTime createdAtUtc,
        string buyerName,
        string buyerPhone,
        bool isGift,
        string? recipientName,
        string? recipientPhone,
        string? recipientCity,
        string? recipientPickupPoint,
        string? giftMessage)
    {
        Validate(waveId, number, buyerName, buyerPhone);

        WaveId = waveId;
        Number = number.Trim().ToUpperInvariant();
        CreatedAtUtc = createdAtUtc;
        BuyerName = buyerName.Trim();
        BuyerPhone = buyerPhone.Trim();
        IsGift = isGift;
        RecipientName = isGift ? RequireValue(recipientName, "Для подарка укажите имя получателя.") : BuyerName;
        RecipientPhone = isGift ? RequireValue(recipientPhone, "Для подарка укажите телефон получателя.") : BuyerPhone;
        RecipientCity = RequireValue(recipientCity, "Укажите город получателя.");
        RecipientPickupPoint = recipientPickupPoint?.Trim() ?? string.Empty;
        GiftMessage = isGift && !string.IsNullOrWhiteSpace(giftMessage) ? giftMessage.Trim() : null;
        Status = OrderStatus.Accepted;
        _history.Add(OrderStatusHistoryEntry.Create(OrderStatus.Accepted, createdAtUtc));
    }

    /// <summary>
    /// Order identifier.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Human friendly order number, for example <c>SW-7K4M2P</c>.
    /// </summary>
    public string Number { get; private set; } = string.Empty;

    /// <summary>
    /// Creation moment, in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Buyer name.
    /// </summary>
    public string BuyerName { get; private set; } = string.Empty;

    /// <summary>
    /// Buyer phone number.
    /// </summary>
    public string BuyerPhone { get; private set; } = string.Empty;

    /// <summary>
    /// Indicates that the order is a gift for another person.
    /// </summary>
    public bool IsGift { get; private set; }

    /// <summary>
    /// Recipient name: the buyer himself for a personal order.
    /// </summary>
    public string RecipientName { get; private set; } = string.Empty;

    /// <summary>
    /// Recipient phone number: the buyer phone for a personal order.
    /// </summary>
    public string RecipientPhone { get; private set; } = string.Empty;

    /// <summary>
    /// Recipient city.
    /// </summary>
    public string RecipientCity { get; private set; } = string.Empty;

    /// <summary>
    /// Pickup point of the delivery company.
    /// </summary>
    public string RecipientPickupPoint { get; private set; } = string.Empty;

    /// <summary>
    /// Text of the greeting card, available for gifts only.
    /// </summary>
    public string? GiftMessage { get; private set; }

    /// <summary>
    /// Identifier of the wave the order travels in.
    /// </summary>
    public int WaveId { get; private set; }

    /// <summary>
    /// Current order status.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Total weight of the order, in grams.
    /// </summary>
    public int TotalWeightGrams { get; private set; }

    /// <summary>
    /// Cost of the goods, in rubles.
    /// </summary>
    public decimal GoodsCost { get; private set; }

    /// <summary>
    /// Delivery cost, in rubles.
    /// </summary>
    public decimal ShippingCost { get; private set; }

    /// <summary>
    /// Thermal packaging cost, in rubles.
    /// </summary>
    public decimal ThermoPackCost { get; private set; }

    /// <summary>
    /// Discount applied to the delivery cost because of the wave fill, in percent.
    /// </summary>
    public decimal AppliedDiscountPercent { get; private set; }

    /// <summary>
    /// Order positions.
    /// </summary>
    public IReadOnlyList<OrderItem> Items => _items;

    /// <summary>
    /// Status change timeline.
    /// </summary>
    public IReadOnlyList<OrderStatusHistoryEntry> History => _history;

    /// <summary>
    /// Total amount payable, in rubles.
    /// </summary>
    public decimal TotalCost => GoodsCost + ShippingCost + ThermoPackCost;

    /// <summary>
    /// Creates an order in the <see cref="OrderStatus.Accepted"/> status with a first history entry.
    /// </summary>
    /// <param name="waveId">Identifier of the chosen wave.</param>
    /// <param name="number">Unique order number.</param>
    /// <param name="createdAtUtc">Creation moment, in UTC.</param>
    /// <param name="buyerName">Buyer name.</param>
    /// <param name="buyerPhone">Buyer phone number.</param>
    /// <param name="isGift">Indicates that the order is a gift.</param>
    /// <param name="recipientName">Recipient name, required for gifts.</param>
    /// <param name="recipientPhone">Recipient phone number, required for gifts.</param>
    /// <param name="recipientCity">Recipient city.</param>
    /// <param name="recipientPickupPoint">Pickup point.</param>
    /// <param name="giftMessage">Greeting card text, available for gifts only.</param>
    /// <returns>Created order.</returns>
    /// <exception cref="DomainException">Order data is not valid.</exception>
    public static Order Create(
        int waveId,
        string number,
        DateTime createdAtUtc,
        string buyerName,
        string buyerPhone,
        bool isGift,
        string? recipientName,
        string? recipientPhone,
        string? recipientCity,
        string? recipientPickupPoint,
        string? giftMessage) =>
        new(
            waveId,
            number,
            createdAtUtc,
            buyerName,
            buyerPhone,
            isGift,
            recipientName,
            recipientPhone,
            recipientCity,
            recipientPickupPoint,
            giftMessage);

    /// <summary>
    /// Adds a position and keeps the order weight and goods cost up to date.
    /// </summary>
    /// <param name="item">Position to add.</param>
    /// <exception cref="DomainException">Position data is not valid.</exception>
    public void AddItem(OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_items.Any(existing => string.Equals(existing.ProductId, item.ProductId, StringComparison.Ordinal)))
        {
            throw new DomainException("Один и тот же товар нельзя добавить в заказ дважды.");
        }

        _items.Add(item);
        TotalWeightGrams += item.LineWeightGrams;
        GoodsCost += item.LineTotal;
    }

    /// <summary>
    /// Fixes the delivery cost calculated for the order at the moment of ordering.
    /// </summary>
    /// <param name="shippingCost">Delivery cost, in rubles.</param>
    /// <param name="thermoPackCost">Thermal packaging cost, in rubles.</param>
    /// <param name="appliedDiscountPercent">Applied delivery discount, in percent.</param>
    /// <exception cref="DomainException">Cost values are not valid.</exception>
    public void ApplyShippingCosts(decimal shippingCost, decimal thermoPackCost, decimal appliedDiscountPercent)
    {
        if (_items.Count == 0)
        {
            throw new DomainException("Нельзя рассчитать доставку для пустого заказа.");
        }

        if (shippingCost < 0m)
        {
            throw new DomainException("Стоимость доставки не может быть отрицательной.");
        }

        if (thermoPackCost < 0m)
        {
            throw new DomainException("Стоимость термоупаковки не может быть отрицательной.");
        }

        if (appliedDiscountPercent is < 0m or > 100m)
        {
            throw new DomainException("Скидка на доставку должна быть в диапазоне от 0 до 100 процентов.");
        }

        ShippingCost = shippingCost;
        ThermoPackCost = thermoPackCost;
        AppliedDiscountPercent = appliedDiscountPercent;
    }

    /// <summary>
    /// Moves the order to the next status of the lifecycle and records the change in the history.
    /// </summary>
    /// <param name="nowUtc">Moment of the change, in UTC.</param>
    /// <exception cref="DomainException">The order is already in the final status.</exception>
    public void AdvanceStatus(DateTime nowUtc)
    {
        OrderStatus? next = Status switch
        {
            OrderStatus.Accepted => OrderStatus.Assembling,
            OrderStatus.Assembling => OrderStatus.InTransit,
            OrderStatus.InTransit => OrderStatus.ReadyForPickup,
            OrderStatus.ReadyForPickup => OrderStatus.Received,
            _ => null
        };

        if (next is null)
        {
            throw new DomainException("Заказ уже получен, дальнейшая смена статуса невозможна.");
        }

        MoveTo(next.Value, nowUtc);
    }

    /// <summary>
    /// Moves the order to <see cref="OrderStatus.InTransit"/> when its wave is dispatched.
    /// </summary>
    /// <param name="nowUtc">Moment of the change, in UTC.</param>
    /// <exception cref="DomainException">The order cannot be shipped in its current status.</exception>
    public void Ship(DateTime nowUtc)
    {
        if (Status is not (OrderStatus.Accepted or OrderStatus.Assembling))
        {
            throw new DomainException("Отправка волны доступна только для принятых и собираемых заказов.");
        }

        MoveTo(OrderStatus.InTransit, nowUtc);
    }

    private void MoveTo(OrderStatus status, DateTime nowUtc)
    {
        Status = status;
        _history.Add(OrderStatusHistoryEntry.Create(status, nowUtc));
    }

    private static void Validate(int waveId, string number, string buyerName, string buyerPhone)
    {
        if (waveId <= 0)
        {
            throw new DomainException("Заказ должен быть привязан к волне.");
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            throw new DomainException("У заказа должен быть номер.");
        }

        if (string.IsNullOrWhiteSpace(buyerName))
        {
            throw new DomainException("Укажите имя покупателя.");
        }

        if (string.IsNullOrWhiteSpace(buyerPhone))
        {
            throw new DomainException("Укажите телефон покупателя.");
        }
    }

    private static string RequireValue(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(errorMessage);
        }

        return value.Trim();
    }
}