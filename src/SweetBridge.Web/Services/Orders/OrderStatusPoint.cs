using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Orders;

/// <summary>
/// Одна точка ленты смены статусов заказа.
/// </summary>
/// <param name="status">Статус, в который перешёл заказ.</param>
/// <param name="changedAtUtc">Момент изменения в UTC.</param>
public sealed record OrderStatusPoint(OrderStatus Status, DateTime ChangedAtUtc);
