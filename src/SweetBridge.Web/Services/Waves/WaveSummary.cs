using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Waves;

/// <summary>
/// Сводка волны для админки и витрины.
/// </summary>
/// <param name="Id">Идентификатор волны.</param>
/// <param name="Number">Сквозной номер волны.</param>
/// <param name="Direction">Направление доставки.</param>
/// <param name="ShipDate">Дата отправки.</param>
/// <param name="OrderCutoffDate">Дедлайн приёма заказов.</param>
/// <param name="CapacityGrams">Вместимость волны в граммах.</param>
/// <param name="MinLoadGrams">Минимальная загрузка в граммах.</param>
/// <param name="LoadedGrams">Текущий заполненный вес в граммах.</param>
/// <param name="FillPercent">Заполнение волны в процентах.</param>
/// <param name="Status">Статус волны.</param>
/// <param name="IsReadyToDispatch">Набрана ли минимальная загрузка.</param>
public sealed record WaveSummary(
    int Id,
    int Number,
    ShippingDirection Direction,
    DateOnly ShipDate,
    DateOnly OrderCutoffDate,
    int CapacityGrams,
    int MinLoadGrams,
    int LoadedGrams,
    decimal FillPercent,
    WaveStatus Status,
    bool IsReadyToDispatch);