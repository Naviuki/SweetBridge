using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Extensions;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Orders;
using SweetBridge.Web.Services.Waves;

namespace SweetBridge.Web.Pages.Admin.Orders;

/// <summary>
/// Список заказов с фильтрами по статусу и волне и кнопкой перехода к следующему статусу.
/// </summary>
/// <param name="orderService">Сервис заказов.</param>
/// <param name="waveService">Сервис волн.</param>
public sealed class IndexModel(IOrderService orderService, IWaveService waveService) : PageModel
{
    /// <summary>
    /// Выбранный фильтр по статусу.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public OrderStatus? Status { get; set; }

    /// <summary>
    /// Выбранный фильтр по волне.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int? WaveId { get; set; }

    /// <summary>
    /// Заказы, подходящие под фильтр.
    /// </summary>
    public IReadOnlyList<OrderSummary> Orders { get; private set; } = [];

    /// <summary>
    /// Волны, доступные в фильтре.
    /// </summary>
    public IReadOnlyList<WaveSummary> Waves { get; private set; } = [];

    /// <summary>
    /// Загружает заказы и данные фильтра.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Orders = await orderService.GetOrdersAsync(new OrderFilter(Status, WaveId), cancellationToken);
        Waves = await waveService.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Переводит заказ на следующий статус.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Возврат к списку.</returns>
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
