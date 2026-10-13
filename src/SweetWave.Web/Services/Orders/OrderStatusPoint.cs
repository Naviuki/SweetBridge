using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Orders;

/// <summary>
/// One point of the order status timeline.
/// </summary>
/// <param name="Status">Status the order moved to.</param>
/// <param name="ChangedAtUtc">Moment of the change, in UTC.</param>
public sealed record OrderStatusPoint(OrderStatus Status, DateTime ChangedAtUtc);