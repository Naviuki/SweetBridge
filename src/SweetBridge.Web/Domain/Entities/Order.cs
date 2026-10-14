using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Заказ покупателя в составе волны.
/// </summary>
public sealed class Order
{
    private readonly List<OrderItem> _items = [];
    private readonly List<OrderStatusHistoryEntry> _history = [];

    private Order()
    {
    }

    private Order(
        string number,
        string? userId,
        DateTime createdAtUtc,
        string buyerName,
        string buyerPhone,
        bool isGift,
        string? recipientName,
        string? recipientPhone,
        string recipientCity,
        int? pickupPointId,
        string? recipientPickupPoint,
        string? giftMessage,
        int waveId)
    {
        Number = number;
        UserId = userId;
        CreatedAtUtc = createdAtUtc;
        BuyerName = buyerName;
        BuyerPhone = buyerPhone;
        IsGift = isGift;
        RecipientName = isGift ? recipientName! : buyerName;
        RecipientPhone = isGift ? recipientPhone! : buyerPhone;
        RecipientCity = recipientCity;
        PickupPointId = pickupPointId;
        RecipientPickupPoint = recipientPickupPoint;
        GiftMessage = isGift ? giftMessage : null;
        WaveId = waveId;
        Status = OrderStatus.Accepted;
        PaymentStatus = PaymentStatus.Pending;
        _history.Add(OrderStatusHistoryEntry.Create(OrderStatus.Accepted, createdAtUtc));
    }

    /// <summary>
    /// Идентификатор заказа.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Номер заказа вида <c>SW-XXXXXX</c>.
    /// </summary>
    public string Number { get; private set; } = string.Empty;

    /// <summary>
    /// Идентификатор пользователя, <see langword="null"/> для гостя.
    /// </summary>
    public string? UserId { get; private set; }

    /// <summary>
    /// Момент создания, в UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Имя покупателя.
    /// </summary>
    public string BuyerName { get; private set; } = string.Empty;

    /// <summary>
    /// Нормализованный телефон покупателя.
    /// </summary>
    public string BuyerPhone { get; private set; } = string.Empty;

    /// <summary>
    /// Признак подарка.
    /// </summary>
    public bool IsGift { get; private set; }

    /// <summary>
    /// Имя получателя.
    /// </summary>
    public string RecipientName { get; private set; } = string.Empty;

    /// <summary>
    /// Нормализованный телефон получателя.
    /// </summary>
    public string RecipientPhone { get; private set; } = string.Empty;

    /// <summary>
    /// Город получателя.
    /// </summary>
    public string RecipientCity { get; private set; } = string.Empty;

    /// <summary>
    /// Выбранный пункт выдачи.
    /// </summary>
    public int? PickupPointId { get; private set; }

    /// <summary>
    /// Адрес пункта выдачи на момент оформления.
    /// </summary>
    public string? RecipientPickupPoint { get; private set; }

    /// <summary>
    /// Текст открытки для подарка.
    /// </summary>
    public string? GiftMessage { get; private set; }

    /// <summary>
    /// Волна, в которой едет заказ.
    /// </summary>
    public int WaveId { get; private set; }

    /// <summary>
    /// Статус заказа.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Статус оплаты.
    /// </summary>
    public PaymentStatus PaymentStatus { get; private set; }

    /// <summary>
    /// Срок, до которого действует резерв веса в волне.
    /// </summary>
    public DateTime? PaymentDueAtUtc { get; private set; }

    /// <summary>
    /// Общий вес заказа в граммах.
    /// </summary>
    public int TotalWeightGrams { get; private set; }

    /// <summary>
    /// Стоимость товаров в рублях.
    /// </summary>
    public decimal GoodsCost { get; private set; }

    /// <summary>
    /// Скидка по промокоду в рублях.
    /// </summary>
    public decimal DiscountAmount { get; private set; }

    /// <summary>
    /// Применённая скидка за заполнение волны в процентах.
    /// </summary>
    public decimal FillDiscountPercent { get; private set; }

    /// <summary>
    /// Стоимость доставки в рублях.
    /// </summary>
    public decimal ShippingCost { get; private set; }

    /// <summary>
    /// Стоимость термоупаковки в рублях.
    /// </summary>
    public decimal ThermoPackCost { get; private set; }

    /// <summary>
    /// Применённый промокод.
    /// </summary>
    public string? PromoCode { get; private set; }

    /// <summary>
    /// Позиции заказа.
    /// </summary>
    public IReadOnlyList<OrderItem> Items => _items;

    /// <summary>
    /// История смены статусов.
    /// </summary>
    public IReadOnlyList<OrderStatusHistoryEntry> History => _history;

    /// <summary>
    /// Итоговая сумма к оплате.
    /// </summary>
    public decimal Total => GoodsCost - DiscountAmount + ShippingCost + ThermoPackCost;

    /// <summary>
    /// Создаёт заказ в статусе «Принят» с первой записью истории.
    /// </summary>
    /// <param name="number">Номер заказа.</param>
    /// <param name="userId">Пользователь.</param>
    /// <param name="createdAtUtc">Момент создания.</param>
    /// <param name="buyerName">Имя покупателя.</param>
    /// <param name="buyerPhone">Телефон покупателя.</param>
    /// <param name="isGift">Признак подарка.</param>
    /// <param name="recipientName">Имя получателя.</param>
    /// <param name="recipientPhone">Телефон получателя.</param>
    /// <param name="recipientCity">Город получателя.</param>
    /// <param name="pickupPointId">Пункт выдачи.</param>
    /// <param name="recipientPickupPoint">Адрес пункта выдачи.</param>
    /// <param name="giftMessage">Текст открытки.</param>
    /// <param name="waveId">Волна.</param>
    /// <returns>Новый заказ.</returns>
    /// <exception cref="DomainException">Данные заказа некорректны.</exception>
    public static Order Create(
        string number,
        string? userId,
        DateTime createdAtUtc,
        string buyerName,
        string buyerPhone,
        bool isGift,
        string? recipientName,
        string? recipientPhone,
        string recipientCity,
        int? pickupPointId,
        string? recipientPickupPoint,
        string? giftMessage,
        int waveId)
    {
        if (waveId <= 0)
        {
            throw new DomainException("Заказ должен быть привязан к волне.");
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            throw new DomainException("У заказа должен быть номер.");
        }

        Require(buyerName, "Укажите имя покупателя.");
        Require(buyerPhone, "Укажите телефон покупателя.");
        Require(recipientCity, "Укажите город получателя.");

        if (isGift)
        {
            Require(recipientName, "Для подарка укажите имя получателя.");
            Require(recipientPhone, "Для подарка укажите телефон получателя.");

            if (giftMessage is { Length: > 300 })
            {
                throw new DomainException("Текст открытки не должен быть длиннее 300 символов.");
            }
        }

        return new Order(
            number.Trim().ToUpperInvariant(),
            userId,
            createdAtUtc,
            buyerName.Trim(),
            buyerPhone.Trim(),
            isGift,
            recipientName,
            recipientPhone,
            recipientCity,
            pickupPointId,
            recipientPickupPoint,
            giftMessage,
            waveId);
    }

    /// <summary>
    /// Добавляет позицию и поддерживает вес и стоимость товаров.
    /// </summary>
    /// <param name="item">Позиция заказа.</param>
    /// <exception cref="DomainException">Позиция уже добавлена.</exception>
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
    /// Фиксирует рассчитанные стоимости доставки, скидки и промокода.
    /// </summary>
    /// <param name="shippingCost">Стоимость доставки.</param>
    /// <param name="thermoPackCost">Стоимость термоупаковки.</param>
    /// <param name="fillDiscountPercent">Скидка за заполнение волны.</param>
    /// <param name="discountAmount">Скидка по промокоду.</param>
    /// <param name="promoCode">Промокод.</param>
    /// <exception cref="DomainException">Значения стоимости некорректны.</exception>
    public void ApplyCosts(
        decimal shippingCost,
        decimal thermoPackCost,
        decimal fillDiscountPercent,
        decimal discountAmount,
        string? promoCode)
    {
        if (_items.Count == 0)
        {
            throw new DomainException("Нельзя рассчитать доставку для пустого заказа.");
        }

        if (shippingCost < 0m || thermoPackCost < 0m)
        {
            throw new DomainException("Стоимость доставки и термоупаковки не может быть отрицательной.");
        }

        if (fillDiscountPercent is < 0m or > 100m)
        {
            throw new DomainException("Скидка за заполнение должна быть в диапазоне от 0 до 100 процентов.");
        }

        if (discountAmount < 0m)
        {
            throw new DomainException("Скидка по промокоду не может быть отрицательной.");
        }

        if (discountAmount > GoodsCost)
        {
            throw new DomainException("Скидка по промокоду не может быть больше стоимости товаров.");
        }

        ShippingCost = shippingCost;
        ThermoPackCost = thermoPackCost;
        FillDiscountPercent = fillDiscountPercent;
        DiscountAmount = discountAmount;
        PromoCode = promoCode;
    }

    /// <summary>
    /// Назначает срок резерва оплаты.
    /// </summary>
    /// <param name="paymentDueAtUtc">Момент, до которого действует резерв.</param>
    public void SetPaymentDeadline(DateTime paymentDueAtUtc) => PaymentDueAtUtc = paymentDueAtUtc;

    /// <summary>
    /// Фиксирует успешную оплату.
    /// </summary>
    /// <exception cref="DomainException">Заказ отменён или уже отменённую оплату провести нельзя.</exception>
    public void MarkPaid()
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new DomainException("Отменённый заказ оплатить нельзя.");
        }

        PaymentStatus = PaymentStatus.Paid;
        PaymentDueAtUtc = null;
    }

    /// <summary>
    /// Проверяет, может ли заказ отменить покупатель.
    /// </summary>
    /// <returns><see langword="true"/>, если заказ ещё можно отменить.</returns>
    public bool CanBeCancelledByCustomer() => Status == OrderStatus.Accepted;

    /// <summary>
    /// Отменяет заказ.
    /// </summary>
    /// <param name="nowUtc">Момент отмены.</param>
    /// <param name="byCustomer">Отменяет покупатель.</param>
    /// <exception cref="DomainException">Заказ нельзя отменить.</exception>
    public void Cancel(DateTime nowUtc, bool byCustomer)
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new DomainException("Заказ уже отменён.");
        }

        if (byCustomer && !CanBeCancelledByCustomer())
        {
            throw new DomainException("Отменить можно только заказ в статусе «Принят».");
        }

        if (Status is OrderStatus.InTransit or OrderStatus.ReadyForPickup or OrderStatus.Received)
        {
            throw new DomainException("Заказ уже отправлен, отменить его нельзя.");
        }

        Status = OrderStatus.Cancelled;
        _history.Add(OrderStatusHistoryEntry.Create(OrderStatus.Cancelled, nowUtc));
    }

    /// <summary>
    /// Переводит заказ на следующий статус жизненного цикла.
    /// </summary>
    /// <param name="nowUtc">Момент изменения.</param>
    /// <exception cref="DomainException">Заказ в финальном статусе.</exception>
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
            throw new DomainException("Заказ уже получен или отменён: дальнейшая смена статуса невозможна.");
        }

        MoveTo(next.Value, nowUtc);
    }

    /// <summary>
    /// Переводит заказ в статус «В пути» при отправке волны.
    /// </summary>
    /// <param name="nowUtc">Момент изменения.</param>
    /// <exception cref="DomainException">Заказ нельзя отправить.</exception>
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

    private static void Require(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(message);
        }
    }
}