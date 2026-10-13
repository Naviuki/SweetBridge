using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Services.Cart;

namespace SweetWave.Web.Pages;

/// <summary>
/// Shopping cart: quantity changes, removal and the totals.
/// </summary>
/// <param name="cartService">Cart service.</param>
public sealed class CartModel(ICartService cartService) : PageModel
{
    /// <summary>
    /// Current cart.
    /// </summary>
    public CartView Cart { get; private set; } = new([]);

    /// <summary>
    /// Loads the cart.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Cart = await cartService.GetCartAsync(cancellationToken);
    }

    /// <summary>
    /// Sets the quantity of a line and returns to the cart.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="quantity">New quantity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect back to the cart.</returns>
    public async Task<IActionResult> OnPostUpdateAsync(string productId, int quantity, CancellationToken cancellationToken)
    {
        await ChangeCartAsync(() => cartService.SetQuantityAsync(productId, quantity, cancellationToken));

        return RedirectToPage();
    }

    /// <summary>
    /// Removes a line and returns to the cart.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect back to the cart.</returns>
    public async Task<IActionResult> OnPostRemoveAsync(string productId, CancellationToken cancellationToken)
    {
        await ChangeCartAsync(() => cartService.RemoveItemAsync(productId, cancellationToken));

        return RedirectToPage();
    }

    /// <summary>
    /// Removes all lines and returns to the cart.
    /// </summary>
    /// <returns>Redirect back to the cart.</returns>
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