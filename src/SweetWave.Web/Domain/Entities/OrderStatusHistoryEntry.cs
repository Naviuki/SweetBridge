using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Domain.Entities;

/// <summary>
/// One entry of the order status timeline.
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
    /// Entry identifier.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Status the order moved to.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Moment of the change, in UTC.
    /// </summary>
    public DateTime ChangedAtUtc { get; private set; }

    /// <summary>
    /// Creates a history entry.
    /// </summary>
    /// <param name="status">Status the order moved to.</param>
    /// <param name="changedAtUtc">Moment of the change, in UTC.</param>
    /// <returns>Created history entry.</returns>
    public static OrderStatusHistoryEntry Create(OrderStatus status, DateTime changedAtUtc) =>
        new(status, changedAtUtc);
}