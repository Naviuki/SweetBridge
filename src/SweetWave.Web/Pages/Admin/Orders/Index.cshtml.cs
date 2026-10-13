using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Extensions;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Services.Orders;
using SweetWave.Web.Services.Waves;

namespace SweetWave.Web.Pages.Admin.Orders;

/// <summary>
/// Order list with the status and wave filters and the button for the next status.
/// </summary>
/// <param name="orderService">Order service.</param>
/// <param name="waveService">Wave service.</param>
public sealed class IndexModel(IOrderService orderService, IWaveService waveService) : PageModel
{
    /// <summary>
    /// Selected status filter.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public OrderStatus? Status { get; set; }

    /// <summary>
    /// Selected wave filter.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int? WaveId { get; set; }

    /// <summary>
    /// Orders that match the filter.
    /// </summary>
    public IReadOnlyList<OrderSummary> Orders { get; private set; } = [];

    /// <summary>
    /// Waves available in the filter.
    /// </summary>
    public IReadOnlyList<WaveSummary> Waves { get; private set; } = [];

    /// <summary>
    /// Loads the orders and the filter data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Orders = await orderService.GetOrdersAsync(new OrderFilter(Status, WaveId), cancellationToken);
        Waves = await waveService.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Moves the order to the next status.
    /// </summary>
    /// <param name="orderId">Order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect back to the list.</returns>
    public async Task<IActionResult> OnPostAdvanceAsync(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            OrderSummary order = await orderService.AdvanceStatusAsync(orderId, cancellationToken);
            TempData["Success"] = $"Заказ {order.Number}: статус «{order.Status.ToDisplayName()}».";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage(new { Status, WaveId });
    }
}