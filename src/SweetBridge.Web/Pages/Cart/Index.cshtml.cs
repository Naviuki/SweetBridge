using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using SweetBridge.Web.Services.Cart;

namespace SweetBridge.Web.Pages.Cart;

/// <summary>
/// Корзина покупателя: изменение количества, удаление позиций и переход к оформлению.
/// </summary>
public class IndexModel : PageModel
{
    private readonly ICartService _cartService;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="IndexModel"/>.
    /// </summary>
    /// <param name="cartService">Сервис корзины.</param>
    public IndexModel(ICartService cartService)
    {
        ArgumentNullException.ThrowIfNull(cartService);

        _cartService = cartService;
    }

    /// <summary>
    /// Корзина с актуальными ценами.
    /// </summary>
    public CartView Cart { get; private set; } = CartView.Empty();

    /// <summary>
    /// Загружает корзину.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Cart = await _cartService.GetCartAsync(cancellationToken);

    /// <summary>
    /// Задаёт количество позиции.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="quantity">Новое количество.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление в корзину.</returns>
    public async Task<IActionResult> OnPostUpdateAsync(
        string productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        Cart = quantity < 1
            ? await _cartService.RemoveItemAsync(productId, cancellationToken)
            : await _cartService.SetQuantityAsync(productId, quantity, cancellationToken);

        return RedirectToPage();
    }

    /// <summary>
    /// Удаляет позицию из корзины.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление в корзину.</returns>
    public async Task<IActionResult> OnPostRemoveAsync(
        string productId,
        CancellationToken cancellationToken)
    {
        Cart = await _cartService.RemoveItemAsync(productId, cancellationToken);

        return RedirectToPage();
    }

    /// <summary>
    /// Очищает корзину.
    /// </summary>
    /// <returns>Перенаправление в корзину.</returns>
    public IActionResult OnPostClear()
    {
        _cartService.Clear();

        return RedirectToPage();
    }
}