using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Cart;
using SweetBridge.Web.Services.Gifts;

namespace SweetBridge.Web.Pages.Gifts;

/// <summary>
/// Витрина подарочных наборов.
/// </summary>
/// <param name="giftSetService">Сервис подарочных наборов.</param>
/// <param name="cartService">Сервис корзины.</param>
public sealed class IndexModel(IGiftSetService giftSetService, ICartService cartService) : PageModel
{
    /// <summary>
    /// Активные наборы.
    /// </summary>
    public IReadOnlyList<GiftSetSummary> Sets { get; private set; } = [];

    /// <summary>
    /// Загружает наборы.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Sets = await giftSetService.GetAllAsync(cancellationToken);

    /// <summary>
    /// Добавляет состав набора в корзину как подарок.
    /// </summary>
    /// <param name="id">Идентификатор набора.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на страницу оформления подарка.</returns>
    public async Task<IActionResult> OnPostAddToCartAsync(int id, CancellationToken cancellationToken)
    {
        GiftSetSummary? set = await giftSetService.GetAsync(id, cancellationToken);

        if (set is null)
        {
            TempData["Error"] = "Набор больше не доступен.";

            return RedirectToPage();
        }

        foreach (GiftSetItemSummary item in set.Items)
        {
            try
            {
                cartService.Add(item.ProductId, item.Quantity);
            }
            catch (DomainException exception)
            {
                TempData["Error"] = exception.Message;

                return RedirectToPage();
            }
        }

        TempData["Success"] = $"Набор «{set.Name}» в корзине. При оформлении отметьте, что это подарок.";

        return RedirectToPage("/Checkout/Index");
    }
}