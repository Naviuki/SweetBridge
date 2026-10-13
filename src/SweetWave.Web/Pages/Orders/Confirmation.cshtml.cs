using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Services.Orders;

namespace SweetWave.Web.Pages.Orders;

/// <summary>
/// Страница подтверждения заказа с его номером.
/// </summary>
/// <param name="orderService">Сервис заказов.</param>
public sealed class ConfirmationModel(IOrderService orderService) : PageModel
{
    /// <summary>
    /// Размещённый заказ.
    /// </summary>
    public OrderSummary? Order { get; private set; }

    /// <summary>
    /// Загружает заказ по его номеру.
    /// </summary>
    /// <param name="number">Номер заказа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    public async Task<IActionResult> OnGetAsync(string? number, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return RedirectToPage("/Index");
        }

        Order = await orderService.FindByNumberAsync(number, cancellationToken);

        return Order is null ? NotFound() : Page();
    }
}
