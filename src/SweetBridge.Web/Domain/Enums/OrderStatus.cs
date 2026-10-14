namespace SweetBridge.Web.Domain.Enums;

/// <summary>
/// Жизненный цикл заказа. Статус движется только вперёд, по одному шагу.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Заказ принят и ждёт сборки волны.
    /// </summary>
    Accepted = 0,

    /// <summary>
    /// Заказ упаковывается в волну.
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
    /// Заказ получен покупателем или получателем подарка.
    /// </summary>
    Received = 4
}
