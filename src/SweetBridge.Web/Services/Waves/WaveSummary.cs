using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Waves;

/// <summary>
/// Модель чтения волны для списка админки.
/// </summary>
/// <param name="waveId">Идентификатор волны.</param>
/// <param name="direction">Направление доставки.</param>
/// <param name="shipDate">Запланированная дата отправки.</param>
/// <param name="orderCutoffDate">Последний день приёма заказов.</param>
/// <param name="capacityGrams">Максимальный вес волны в граммах.</param>
/// <param name="minLoadGrams">Минимальный вес для отправки в граммах.</param>
/// <param name="loadedGrams">Вес, уже набранный в волне, в граммах.</param>
/// <param name="FillPercent">Заполнение волны в процентах.</param>
/// <param name="status">Статус волны.</param>
/// <param name="IsReadyToDispatch">Признак того, что в волне есть минимальная загрузка и её можно отправить.</param>
public sealed record WaveSummary(
    int WaveId,
    ShippingDirection Direction,
    DateOnly ShipDate,
    DateOnly OrderCutoffDate,
    int CapacityGrams,
    int MinLoadGrams,
    int LoadedGrams,
    decimal FillPercent,
    WaveStatus Status,
    bool IsReadyToDispatch);
