namespace SweetBridge.Web.Domain.Enums;

/// <summary>
/// Статус заказа. Первые пять статусов образуют цепочку доставки, последний — отмена.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Заказ принят и ждёт сборки волны.
    /// </summary>
    Accepted = 0,

    /// <summary>
    /// Идёт комплектация партии.
    /// </summary>
    Assembling = 1,

    /// <summary>
    /// Волна в пути.
    /// </summary>
    InTransit = 2,

    /// <summary>
    /// Заказ прибыл в пункт выдачи.
    /// </summary>
    ReadyForPickup = 3,

    /// <summary>
    /// Заказ получен.
    /// </summary>
    Received = 4,

    /// <summary>
    /// Заказ отменен.
    /// </summary>
    Cancelled = 5
}