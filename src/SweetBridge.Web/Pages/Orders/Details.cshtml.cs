using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Orders;
using SweetBridge.Web.Services.Reporting;

namespace SweetBridge.Web.Pages.Orders;

/// <summary>
/// Карточка заказа: состав, статусы, оплата, отмена и повторный заказ.
/// </summary>
/// <param name="orderService">Сервис заказов.</param>
/// <param name="cartService">Сервис корзины.</param>
/// <param name="receiptBuilder">Построитель PDF-накладной.</param>
public sealed class DetailsModel(
    IOrderService orderService,
    Services.Cart.ICartService cartService,
    IReceiptBuilder receiptBuilder) : PageModel
{
    /// <summary>
    /// Номер заказа.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Number { get; set; }

    /// <summary>
    /// Заказ.
    /// </summary>
    public OrderSummary? Order { get; private set; }

    /// <summary>
    /// Признак того, что заказ можно отменить покупателем.
    /// </summary>
    public bool CanCancel { get; private set; }

    /// <summary>
    /// Загружает заказ.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Order = await LoadAsync(cancellationToken);

        if (Order is null)
        {
            return NotFound();
        }

        CanCancel = Order.Status == Domain.Enums.OrderStatus.Accepted;

        return Page();
    }

    /// <summary>
    /// Отменяет заказ покупателем.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task<IActionResult> OnPostCancelAsync(CancellationToken cancellationToken)
    {
        try
        {
            await orderService.CancelByCustomerAsync(Order!.Id, cancellationToken);
            TempData["Success"] = "Заказ отменён, вес освобождён в волне.";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Переносит состав заказа в корзину.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление в корзину.</returns>
    public IActionResult OnPostRepeatAsync()
    {
        ArgumentNullException.ThrowIfNull(Order);

        foreach (OrderLineSummary line in Order.Items)
        {
            cartService.Add(line.ProductId, line.Quantity);
        }

        TempData["Success"] = "Состав заказа перенесён в корзину.";

        return RedirectToPage("/Cart/Index");
    }

    /// <summary>
    /// Отдаёт PDF-накладную по заказу.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Файл PDF или 404.</returns>
    public async Task<IActionResult> OnGetReceiptAsync(CancellationToken cancellationToken)
    {
        OrderSummary? order = await LoadAsync(cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        return File(receiptBuilder.Build(order), "application/pdf", $"Заказ-{order.Number}.pdf");
    }

    private async Task<OrderSummary?> LoadAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Number) || !Domain.ValueObjects.OrderNumber.IsValid(Number))
        {
            return null;
        }

        return await orderService.FindByNumberAsync(Number, cancellationToken);
    }
}