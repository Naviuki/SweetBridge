using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Filter of the admin order list.
/// </summary>
/// <param name="Status">Status to filter, or <see langword="null"/> for all statuses.</param>
/// <param name="WaveId">Wave to filter, or <see langword="null"/> for all waves.</param>
public sealed record OrderFilter(OrderStatus? Status = null, int? WaveId = null);