namespace SweetWave.Web.Services.Delivery;

/// <summary>
/// Входные данные для расчёта стоимости доставки.
/// </summary>
/// <param name="WeightGrams">Вес заказа в граммах.</param>
/// <param name="WaveFillPercent">Заполнение волны после добавления заказа в процентах.</param>
/// <param name="ThermoPackRequired">Признак того, что термоупаковку нужно оплатить.</param>
public sealed record DeliveryCostRequest(int WeightGrams, decimal WaveFillPercent, bool ThermoPackRequired);
