using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Orders;

/// <summary>
/// Модель чтения заказа для страниц подтверждения, отслеживания и админки.
/// </summary>
/// <param name="Id">Идентификатор заказа.</param>
/// <param name="number">Номер заказа.</param>
/// <param name="createdAtUtc">Момент создания в UTC.</param>
/// <param name="Status">Текущий статус.</param>
/// <param name="isGift">Признак того, что заказ является подарком.</param>
/// <param name="buyerName">Имя покупателя.</param>
/// <param name="buyerPhone">Телефон покупателя.</param>
/// <param name="RecipientName">Имя получателя.</param>
/// <param name="RecipientPhone">Телефон получателя.</param>
/// <param name="recipientCity">Город получателя.</param>
/// <param name="recipientPickupPoint">Пункт выдачи.</param>
/// <param name="giftMessage">Текст открытки.</param>
/// <param name="waveId">Идентификатор волны.</param>
/// <param name="WaveShipDate">Дата отправки волны.</param>
/// <param name="TotalWeightGrams">Общий вес заказа в граммах.</param>
/// <param name="GoodsCost">Стоимость товаров в рублях.</param>
/// <param name="shippingCost">Стоимость доставки в рублях.</param>
/// <param name="thermoPackCost">Стоимость термоупаковки в рублях.</param>
/// <param name="appliedDiscountPercent">Применённая скидка на доставку в процентах.</param>
/// <param name="Items">Позиции заказа.</param>
/// <param name="History">Лента смены статусов.</param>
public sealed record OrderSummary(
    int Id,
    string Number,
    DateTime CreatedAtUtc,
    OrderStatus Status,
    bool IsGift,
    string BuyerName,
    string BuyerPhone,
    string RecipientName,
    string RecipientPhone,
    string RecipientCity,
    string RecipientPickupPoint,
    string? GiftMessage,
    int WaveId,
    DateOnly WaveShipDate,
    int TotalWeightGrams,
    decimal GoodsCost,
    decimal ShippingCost,
    decimal ThermoPackCost,
    decimal AppliedDiscountPercent,
    IReadOnlyList<OrderLineSummary> Items,
    IReadOnlyList<OrderStatusPoint> History)
{
    /// <summary>
    /// Итоговая сумма к оплате в рублях.
    /// </summary>
    public decimal TotalCost => GoodsCost + ShippingCost + ThermoPackCost;

    /// <summary>
    /// Признак того, что термоупаковка была оплачена.
    /// </summary>
    public bool HasThermoPack => ThermoPackCost > 0m;
}
