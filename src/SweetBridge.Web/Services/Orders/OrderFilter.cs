using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Orders;

/// <summary>
/// Фильтр списка заказов админки.
/// </summary>
/// <param name="Status">Статус для фильтра или <see langword="null"/> для всех статусов.</param>
/// <param name="WaveId">Волна для фильтра или <see langword="null"/> для всех волн.</param>
/// <param name="Search">Фрагмент номера заказа или <see langword="null"/>.</param>
public sealed record OrderFilter(OrderStatus? Status = null, int? WaveId = null, string? Search = null);