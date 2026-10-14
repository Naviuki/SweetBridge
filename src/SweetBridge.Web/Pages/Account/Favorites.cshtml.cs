using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Favorites;

namespace SweetBridge.Web.Pages.Account;

/// <summary>
/// Избранные товары покупателя.
/// </summary>
/// <param name="favoriteService">Сервис избранного.</param>
/// <param name="cartService">Сервис корзины.</param>
public sealed class FavoritesModel(IFavoriteService favoriteService, Services.Cart.ICartService cartService) : PageModel
{
    /// <summary>
    /// Избранные товары.
    /// </summary>
    public IReadOnlyList<ProductCardViewModel> Products { get; private set; } = [];

    /// <summary>
    /// Загружает избранное.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        Products =
        [
            .. (await favoriteService.GetAllAsync(userId, cancellationToken))
                .Select(product => new ProductCardViewModel(
                    product.Id,
                    product.Name,
                    product.Category,
                    product.ProducerName,
                    product.Region,
                    product.WeightGrams,
                    product.Price,
                    product.IsTemperatureSensitive,
                    product.PhotoPath,
                    cartService.GetQuantity(product.Id)))
        ];
    }

    /// <summary>
    /// Убирает товар из избранного.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на страницу избранного.</returns>
    public async Task<IActionResult> OnPostRemoveAsync(
        string productId,
        CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await favoriteService.ToggleAsync(userId, productId, cancellationToken);
            TempData["Info"] = "Товар убран из избранного.";
        }

        return RedirectToPage();
    }
}