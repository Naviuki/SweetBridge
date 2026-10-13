using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Services.Orders;

namespace SweetWave.Web.Pages.Orders;

/// <summary>
/// Order confirmation page with the order number.
/// </summary>
/// <param name="orderService">Order service.</param>
public sealed class ConfirmationModel(IOrderService orderService) : PageModel
{
    /// <summary>
    /// Placed order.
    /// </summary>
    public OrderSummary? Order { get; private set; }

    /// <summary>
    /// Loads the order by its number.
    /// </summary>
    /// <param name="number">Order number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
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