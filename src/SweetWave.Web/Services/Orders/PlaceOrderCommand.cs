namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Данные формы оформления. Цены и веса всегда пересчитываются на сервере.
/// </summary>
/// <param name="WaveId">Выбранная волна.</param>
/// <param name="buyerName">Имя покупателя.</param>
/// <param name="buyerPhone">Телефон покупателя.</param>
/// <param name="isGift">Признак того, что заказ является подарком.</param>
/// <param name="RecipientName">Имя получателя для подарка.</param>
/// <param name="RecipientPhone">Телефон получателя для подарка.</param>
/// <param name="recipientCity">Город получателя.</param>
/// <param name="recipientPickupPoint">Пункт выдачи.</param>
/// <param name="giftMessage">Текст открытки.</param>
/// <param name="thermoPackRequested">Термоупаковка, выбранная покупателем.</param>
/// <param name="Lines">Заказанные позиции.</param>
public sealed record PlaceOrderCommand(
    int WaveId,
    string BuyerName,
    string BuyerPhone,
    bool IsGift,
    string? RecipientName,
    string? RecipientPhone,
    string? RecipientCity,
    string? RecipientPickupPoint,
    string? GiftMessage,
    bool ThermoPackRequested,
    IReadOnlyList<OrderLineRequest> Lines);
