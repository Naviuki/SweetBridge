using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Одна точка ленты смены статусов заказа.
/// </summary>
/// <param name="status">Статус, в который перешёл заказ.</param>
/// <param name="changedAtUtc">Момент изменения в UTC.</param>
public sealed record OrderStatusPoint(OrderStatus Status, DateTime ChangedAtUtc);
