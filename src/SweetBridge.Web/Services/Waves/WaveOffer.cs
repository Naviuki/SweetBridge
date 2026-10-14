using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Services.Delivery;

namespace SweetBridge.Web.Services.Waves;

/// <summary>
/// Предложение волны для покупателя: даты, вместимость и стоимость доставки.
/// </summary>
/// <param name="Id">Идентификатор волны.</param>
/// <param name="Number">Сквозной номер волны.</param>
/// <param name="Direction">Направление доставки.</param>
/// <param name="ShipDate">Дата отправки.</param>
/// <param name="OrderCutoffDate">Дедлайн приёма заказов.</param>
/// <param name="CapacityGrams">Вместимость волны в граммах.</param>
/// <param name="LoadedGrams">Текущий заполненный вес в граммах.</param>
/// <param name="FillPercent">Заполнение волны в процентах.</param>
/// <param name="FillPercentAfter">Заполнение после добавления заказа в процентах.</param>
/// <param name="CanAcceptOrder">Поместится ли заказ в волну.</param>
/// <param name="Cost">Расчёт стоимости доставки.</param>
/// <param name="ThermoPackRequired">Обязательна ли термоупаковка.</param>
/// <param name="MinDeliveryDays">Минимальный срок доставки в днях.</param>
/// <param name="MaxDeliveryDays">Максимальный срок доставки в днях.</param>
/// <param name="EarliestDeliveryDate">Самая ранняя дата доставки.</param>
/// <param name="LatestDeliveryDate">Самая поздняя дата доставки.</param>
public sealed record WaveOffer(
    int Id,
    int Number,
    ShippingDirection Direction,
    DateOnly ShipDate,
    DateOnly OrderCutoffDate,
    int CapacityGrams,
    int LoadedGrams,
    decimal FillPercent,
    decimal FillPercentAfter,
    bool CanAcceptOrder,
    DeliveryCostResult Cost,
    bool ThermoPackRequired,
    int MinDeliveryDays,
    int MaxDeliveryDays,
    DateOnly EarliestDeliveryDate,
    DateOnly LatestDeliveryDate);