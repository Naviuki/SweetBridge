using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Заказ покупателя, размещённый в одной из волн.
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
    /// Идентификатор заказа.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Понятный человеку номер заказа, например <c>SW-7K4M2P</c>.
    /// </summary>
    public string Number { get; private set; } = string.Empty;

    /// <summary>
    /// Момент создания в UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Имя покупателя.
    /// </summary>
    public string BuyerName { get; private set; } = string.Empty;

    /// <summary>
    /// Телефон покупателя.
    /// </summary>
    public string BuyerPhone { get; private set; } = string.Empty;

    /// <summary>
    /// Признак того, что заказ является подарком для другого человека.
    /// </summary>
    public bool IsGift { get; private set; }

    /// <summary>
    /// Имя получателя: для личного заказа это сам покупатель.
    /// </summary>
    public string RecipientName { get; private set; } = string.Empty;

    /// <summary>
    /// Телефон получателя: для личного заказа это телефон покупателя.
    /// </summary>
    public string RecipientPhone { get; private set; } = string.Empty;

    /// <summary>
    /// Город получателя.
    /// </summary>
    public string RecipientCity { get; private set; } = string.Empty;

    /// <summary>
    /// Пункт выдачи службы доставки.
    /// </summary>
    public string RecipientPickupPoint { get; private set; } = string.Empty;

    /// <summary>
    /// Текст открытки, доступен только для подарков.
    /// </summary>
    public string? GiftMessage { get; private set; }

    /// <summary>
    /// Идентификатор волны, в которой едет заказ.
    /// </summary>
    public int WaveId { get; private set; }

    /// <summary>
    /// Текущий статус заказа.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Общий вес заказа в граммах.
    /// </summary>
    public int TotalWeightGrams { get; private set; }

    /// <summary>
    /// Стоимость товаров в рублях.
    /// </summary>
    public decimal GoodsCost { get; private set; }

    /// <summary>
    /// Стоимость доставки в рублях.
    /// </summary>
    public decimal ShippingCost { get; private set; }

    /// <summary>
    /// Стоимость термоупаковки в рублях.
    /// </summary>
    public decimal ThermoPackCost { get; private set; }

    /// <summary>
    /// Скидка на доставку за заполнение волны в процентах.
    /// </summary>
    public decimal AppliedDiscountPercent { get; private set; }

    /// <summary>
    /// Позиции заказа.
    /// </summary>
    public IReadOnlyList<OrderItem> Items => _items;

    /// <summary>
    /// Лента смены статусов.
    /// </summary>
    public IReadOnlyList<OrderStatusHistoryEntry> History => _history;

    /// <summary>
    /// Итоговая сумма к оплате в рублях.
    /// </summary>
    public decimal TotalCost => GoodsCost + ShippingCost + ThermoPackCost;

    /// <summary>
    /// Создаёт заказ в статусе <see cref="OrderStatus.Accepted"/> с первой записью истории.
    /// </summary>
    /// <param name="waveId">Идентификатор выбранной волны.</param>
    /// <param name="number">Уникальный номер заказа.</param>
    /// <param name="createdAtUtc">Момент создания в UTC.</param>
    /// <param name="buyerName">Имя покупателя.</param>
    /// <param name="buyerPhone">Телефон покупателя.</param>
    /// <param name="isGift">Признак того, что заказ является подарком.</param>
    /// <param name="recipientName">Имя получателя, обязательно для подарка.</param>
    /// <param name="recipientPhone">Телефон получателя, обязательно для подарка.</param>
    /// <param name="recipientCity">Город получателя.</param>
    /// <param name="recipientPickupPoint">Пункт выдачи.</param>
    /// <param name="giftMessage">Текст открытки, доступен только для подарка.</param>
    /// <returns>Созданный заказ.</returns>
    /// <exception cref="DomainException">Данные заказа некорректны.</exception>
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
    /// Добавляет позицию и поддерживает вес и стоимость товаров заказа в актуальном состоянии.
    /// </summary>
    /// <param name="item">Добавляемая позиция.</param>
    /// <exception cref="DomainException">Данные позиции заказа некорректны.</exception>
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
    /// Фиксирует рассчитанную для заказа стоимость доставки.
    /// </summary>
    /// <param name="shippingCost">Стоимость доставки в рублях.</param>
    /// <param name="thermoPackCost">Стоимость термоупаковки в рублях.</param>
    /// <param name="appliedDiscountPercent">Применённая скидка на доставку в процентах.</param>
    /// <exception cref="DomainException">Значения стоимости некорректны.</exception>
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
    /// Переводит заказ на следующий статус жизненного цикла и записывает изменение в историю.
    /// </summary>
    /// <param name="nowUtc">Момент изменения в UTC.</param>
    /// <exception cref="DomainException">Заказ уже в финальном статусе.</exception>
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
    /// Переводит заказ в <see cref="OrderStatus.InTransit"/> при отправке его волны.
    /// </summary>
    /// <param name="nowUtc">Момент изменения в UTC.</param>
    /// <exception cref="DomainException">Заказ нельзя отправить в его текущем статусе.</exception>
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
