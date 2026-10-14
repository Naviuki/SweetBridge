using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Services.Orders;

namespace SweetBridge.Web.Pages.Account;

/// <summary>
/// Мои заказы: список заказов текущего покупателя.
/// </summary>
/// <param name="orderService">Сервис заказов.</param>
public sealed class OrdersModel(IOrderService orderService) : PageModel
{
    /// <summary>
    /// Заказы пользователя.
    /// </summary>
    public IReadOnlyList<OrderSummary> Orders { get; private set; } = [];

    /// <summary>
    /// Загружает заказы пользователя.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            Orders = await orderService.GetUserOrdersAsync(userId, cancellationToken);
        }
    }
}