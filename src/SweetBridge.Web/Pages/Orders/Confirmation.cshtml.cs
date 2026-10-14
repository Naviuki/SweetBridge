using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Services.Orders;
using SweetBridge.Web.Services.Reporting;

namespace SweetBridge.Web.Pages.Orders;

/// <summary>
/// Страница подтверждения: показывает номер заказа, состав, оплату и трекер.
/// </summary>
/// <param name="orderService">Сервис заказов.</param>
/// <param name="receiptBuilder">Построитель PDF-накладной.</param>
public sealed class ConfirmationModel(IOrderService orderService, IReceiptBuilder receiptBuilder) : PageModel
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
    /// Загружает заказ.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Order = await LoadAsync(cancellationToken);

        return Order is null ? NotFound() : Page();
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

        byte[] pdf = receiptBuilder.Build(order);

        return File(pdf, "application/pdf", $"Заказ-{order.Number}.pdf");
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