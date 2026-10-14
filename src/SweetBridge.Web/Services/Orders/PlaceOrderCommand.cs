namespace SweetBridge.Web.Services.Orders;

/// <summary>
/// Данные формы оформления. Цены, веса и скидки всегда пересчитываются на сервере.
/// </summary>
/// <param name="WaveId">Выбранная волна.</param>
/// <param name="UserId">Идентификатор авторизованного покупателя.</param>
/// <param name="BuyerName">Имя покупателя.</param>
/// <param name="BuyerPhone">Телефон покупателя.</param>
/// <param name="IsGift">Признак того, что заказ является подарком.</param>
/// <param name="RecipientName">Имя получателя для подарка.</param>
/// <param name="RecipientPhone">Телефон получателя для подарка.</param>
/// <param name="RecipientCity">Город получателя.</param>
/// <param name="PickupPointId">Идентификатор пункта выдачи.</param>
/// <param name="RecipientPickupPoint">Название пункта выдачи.</param>
/// <param name="GiftMessage">Текст открытки.</param>
/// <param name="ThermoPackRequested">Термоупаковка, выбранная покупателем.</param>
/// <param name="PromoCode">Промокод, введённый покупателем.</param>
/// <param name="Lines">Заказанные позиции.</param>
public sealed record PlaceOrderCommand(
    int WaveId,
    string? UserId,
    string BuyerName,
    string BuyerPhone,
    bool IsGift,
    string? RecipientName,
    string? RecipientPhone,
    string? RecipientCity,
    int? PickupPointId,
    string? RecipientPickupPoint,
    string? GiftMessage,
    bool ThermoPackRequested,
    string? PromoCode,
    IReadOnlyList<OrderLineRequest> Lines);