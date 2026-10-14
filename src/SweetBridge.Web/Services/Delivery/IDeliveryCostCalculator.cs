namespace SweetBridge.Web.Services.Delivery;

/// <summary>
/// Рассчитывает стоимость сборной доставки.
/// </summary>
public interface IDeliveryCostCalculator
{
    /// <summary>
    /// Рассчитывает стоимость доставки заказа.
    /// </summary>
    /// <param name="request">Вес, заполнение волны и необходимость термоупаковки.</param>
    /// <returns>Рассчитанные стоимости.</returns>
    DeliveryCostResult Calculate(DeliveryCostRequest request);
}
