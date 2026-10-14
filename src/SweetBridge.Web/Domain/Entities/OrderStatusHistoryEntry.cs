using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Одна запись ленты смены статусов заказа.
/// </summary>
public sealed class OrderStatusHistoryEntry
{
    private OrderStatusHistoryEntry()
    {
    }

    private OrderStatusHistoryEntry(OrderStatus status, DateTime changedAtUtc)
    {
        Status = status;
        ChangedAtUtc = changedAtUtc;
    }

    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Идентификатор заказа, которому принадлежит запись.
    /// </summary>
    public int OrderId { get; private set; }

    /// <summary>
    /// Статус, в который перешёл заказ.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Момент изменения в UTC.
    /// </summary>
    public DateTime ChangedAtUtc { get; private set; }

    /// <summary>
    /// Создаёт запись истории.
    /// </summary>
    /// <param name="status">Статус, в который перешёл заказ.</param>
    /// <param name="changedAtUtc">Момент изменения в UTC.</param>
    /// <returns>Созданная запись истории.</returns>
    public static OrderStatusHistoryEntry Create(OrderStatus status, DateTime changedAtUtc) =>
        new(status, changedAtUtc);
}
