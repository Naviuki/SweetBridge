using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Services.Delivery;

namespace SweetWave.Web.Services.Waves;

/// <summary>
/// Предложение волны для текущей корзины: даты, заполнение, стоимость доставки и срок.
/// </summary>
/// <param name="waveId">Идентификатор волны.</param>
/// <param name="direction">Направление доставки.</param>
/// <param name="shipDate">Запланированная дата отправки.</param>
/// <param name="orderCutoffDate">Последний день приёма заказов.</param>
/// <param name="capacityGrams">Максимальный вес волны в граммах.</param>
/// <param name="loadedGrams">Вес, уже набранный в волне, в граммах.</param>
/// <param name="FillPercentBefore">Заполнение волны до заказа в процентах.</param>
/// <param name="FillPercentAfter">Заполнение волны после заказа в процентах.</param>
/// <param name="CanAcceptOrder">Признак того, что заказ помещается в оставшуюся вместимость.</param>
/// <param name="DeliveryCost">Рассчитанная стоимость доставки.</param>
/// <param name="ThermoPackRequired">Признак того, что термоупаковку нельзя отключить.</param>
/// <param name="DeliveryDaysMin">Минимальный срок доставки в днях.</param>
/// <param name="DeliveryDaysMax">Максимальный срок доставки в днях.</param>
/// <param name="EstimatedDeliveryFrom">Самая ранняя расчётная дата доставки.</param>
/// <param name="EstimatedDeliveryTo">Самая поздняя расчётная дата доставки.</param>
public sealed record WaveOffer(
    int WaveId,
    ShippingDirection Direction,
    DateOnly ShipDate,
    DateOnly OrderCutoffDate,
    int CapacityGrams,
    int LoadedGrams,
    decimal FillPercentBefore,
    decimal FillPercentAfter,
    bool CanAcceptOrder,
    DeliveryCostResult DeliveryCost,
    bool ThermoPackRequired,
    int DeliveryDaysMin,
    int DeliveryDaysMax,
    DateOnly EstimatedDeliveryFrom,
    DateOnly EstimatedDeliveryTo)
{
    /// <summary>
    /// Свободная вместимость волны после заказа в граммах.
    /// </summary>
    public int RemainingGrams => CapacityGrams - LoadedGrams;
}
