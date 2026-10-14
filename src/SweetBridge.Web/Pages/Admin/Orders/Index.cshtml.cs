using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Extensions;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Auditing;
using SweetBridge.Web.Services.Orders;

namespace SweetBridge.Web.Pages.Admin.Orders;

/// <summary>
/// Список заказов администратора со сменой статуса и отменой.
/// </summary>
/// <param name="orderService">Сервис заказов.</param>
/// <param name="auditService">Журнал действий.</param>
public sealed class IndexModel(IOrderService orderService, IAuditService auditService) : PageModel
{
    /// <summary>
    /// Фильтр по статусу.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public OrderStatus? Status { get; set; }

    /// <summary>
    /// Фильтр по номеру заказа.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    [StringLength(16)]
    public string? Search { get; set; }

    /// <summary>
    /// Подходящие заказы.
    /// </summary>
    public IReadOnlyList<OrderSummary> Orders { get; private set; } = [];

    /// <summary>
    /// Все статусы для фильтра.
    /// </summary>
    public IReadOnlyList<SelectItem> Statuses { get; } =
    [
        new SelectItem(null, "Все статусы"),
        .. Enum.GetValues<OrderStatus>().Select(value => new SelectItem(value, value.ToDisplayTitle()))
    ];

    /// <summary>
    /// Загружает список заказов.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Orders = await orderService.GetOrdersAsync(new OrderFilter(Status, null, Search), cancellationToken);

    /// <summary>
    /// Переводит заказ на следующий статус.
    /// </summary>
    /// <param name="id">Идентификатор заказа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на список заказов.</returns>
    public async Task<IActionResult> OnPostAdvanceAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            OrderSummary order = await orderService.AdvanceStatusAsync(id, cancellationToken);
            await auditService.WriteAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system",
                "Order.AdvanceStatus",
                nameof(Order),
                $"Заказ {order.Number}: {order.Status.ToDisplayTitle()}",
                cancellationToken);

            TempData["Success"] = $"Заказ {order.Number}: статус «{order.Status.ToDisplayTitle()}».";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Отменяет заказ администратором.
    /// </summary>
    /// <param name="id">Идентификатор заказа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на список заказов.</returns>
    public async Task<IActionResult> OnPostCancelAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            OrderSummary order = await orderService.CancelByAdminAsync(id, cancellationToken);
            await auditService.WriteAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system",
                "Order.Cancel",
                nameof(Order),
                $"Заказ {order.Number}",
                cancellationToken);

            TempData["Success"] = $"Заказ {order.Number} отменён.";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Пункт выбора в фильтре.
    /// </summary>
    /// <param name="Value">Значение.</param>
    /// <param name="Title">Подпись.</param>
    public sealed record SelectItem(OrderStatus? Value, string Title);
}