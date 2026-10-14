namespace SweetBridge.Web.Domain.Enums;

/// <summary>
/// Статус оплаты заказа.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Ожидает оплаты, вес зарезервирован в волне.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Оплата получена.
    /// </summary>
    Paid = 1,

    /// <summary>
    /// Оплата не прошла.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Оплата возвращена.
    /// </summary>
    Refunded = 3
}