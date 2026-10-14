using SweetBridge.Web.Services.Catalog;

namespace SweetBridge.Web.Services.Seasons;

/// <summary>
/// Сезонные правила: термоупаковка и срок доставки.
/// </summary>
public interface ISeasonPolicy
{
    /// <summary>
    /// Проверяет, обязательна ли термоупаковка для указанных товаров и даты отправки.
    /// </summary>
    /// <param name="products">Товары заказа.</param>
    /// <param name="shipDate">Запланированная дата отправки.</param>
    /// <returns><see langword="true"/>, если термоупаковку нельзя отключить.</returns>
    bool IsThermoPackRequired(IEnumerable<ProductSummary> products, DateOnly shipDate);

    /// <summary>
    /// Возвращает количество дней, добавляемых к сроку доставки в межсезонье.
    /// </summary>
    /// <param name="shipDate">Запланированная дата отправки.</param>
    /// <returns>Количество дополнительных дней.</returns>
    int GetExtraDeliveryDays(DateOnly shipDate);
}
