using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Orders;
using SweetBridge.Web.Services.Payments;

namespace SweetBridge.Web.Pages.Checkout;

/// <summary>
/// Демонстрационная страница оплаты: карточные данные не принимаются,
/// оплата фиксируется через платёжный шлюз-заглушку.
/// </summary>
/// <param name="orderService">Сервис заказов.</param>
/// <param name="paymentGateway">Платёжный шлюз.</param>
public sealed class PayModel(IOrderService orderService, IPaymentGateway paymentGateway) : PageModel
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
    /// Загружает заказ для оплаты.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Order = await LoadAsync(cancellationToken);

        return Order is null ? NotFound() : Page();
    }

    /// <summary>
    /// Проводит оплату и возвращает на страницу подтверждения.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на страницу подтверждения.</returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Order = await LoadAsync(cancellationToken);

        if (Order is null)
        {
            return NotFound();
        }

        if (Order.PaymentStatus == Domain.Enums.PaymentStatus.Paid)
        {
            return RedirectToPage("/Orders/Confirmation", new { number = Order.Number });
        }

        PaymentResult result = await paymentGateway.PayAsync(Order.Number, Order.Total, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Message;

            return RedirectToPage();
        }

        await orderService.MarkPaidAsync(Order.Number, cancellationToken);
        TempData["Success"] = result.Message;

        return RedirectToPage("/Orders/Confirmation", new { number = Order.Number });
    }

    private async Task<OrderSummary?> LoadAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Number) || !Domain.ValueObjects.OrderNumber.IsValid(Number))
        {
            return null;
        }

        string? userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        OrderSummary? order = await orderService.FindByNumberAsync(Number, cancellationToken);

        if (order is null)
        {
            return null;
        }

        if (order.UserId is not null && !order.BelongsTo(userId))
        {
            throw new DomainException("Оплатить этот заказ может только автор заказа.");
        }

        return order;
    }
}