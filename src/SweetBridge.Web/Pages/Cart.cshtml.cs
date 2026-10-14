using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Cart;

namespace SweetBridge.Web.Pages;

/// <summary>
/// Корзина покупок: изменение количества, удаление и итоги.
/// </summary>
/// <param name="cartService">Сервис корзины.</param>
public sealed class CartModel(ICartService cartService) : PageModel
{
    /// <summary>
    /// Текущая корзина.
    /// </summary>
    public CartView Cart { get; private set; } = new([]);

    /// <summary>
    /// Загружает корзину.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Cart = await cartService.GetCartAsync(cancellationToken);
    }

    /// <summary>
    /// Задаёт количество в позиции и возвращает к корзине.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="quantity">Новое количество.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Возврат к корзине.</returns>
    public async Task<IActionResult> OnPostUpdateAsync(string productId, int quantity, CancellationToken cancellationToken)
    {
        await ChangeCartAsync(() => cartService.SetQuantityAsync(productId, quantity, cancellationToken));

        return RedirectToPage();
    }

    /// <summary>
    /// Удаляет позицию и возвращает к корзине.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Возврат к корзине.</returns>
    public async Task<IActionResult> OnPostRemoveAsync(string productId, CancellationToken cancellationToken)
    {
        await ChangeCartAsync(() => cartService.RemoveItemAsync(productId, cancellationToken));

        return RedirectToPage();
    }

    /// <summary>
    /// Удаляет все позиции и возвращает к корзине.
    /// </summary>
    /// <returns>Возврат к корзине.</returns>
    public IActionResult OnPostClear()
    {
        cartService.Clear();
        TempData["Success"] = "Корзина очищена.";

        return RedirectToPage();
    }

    private async Task ChangeCartAsync(Func<Task<CartView>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            await action();
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }
    }
}
