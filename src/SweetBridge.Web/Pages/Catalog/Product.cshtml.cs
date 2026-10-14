using SweetBridge.Web.Domain.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Cart;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Delivery;
using SweetBridge.Web.Services.Favorites;

namespace SweetBridge.Web.Pages.Catalog;

/// <summary>
/// Страница товара: описание, цена, добавление в корзину и избранное.
/// </summary>
public class ProductModel : PageModel
{
    private readonly ICatalogService _catalogService;
    private readonly ICartService _cartService;
    private readonly IFavoriteService _favoriteService;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="ProductModel"/>.
    /// </summary>
    /// <param name="catalogService">Сервис каталога.</param>
    /// <param name="cartService">Сервис корзины.</param>
    /// <param name="favoriteService">Сервис избранного.</param>
    public ProductModel(ICatalogService catalogService, ICartService cartService, IFavoriteService favoriteService)
    {
        ArgumentNullException.ThrowIfNull(catalogService);
        ArgumentNullException.ThrowIfNull(cartService);
        ArgumentNullException.ThrowIfNull(favoriteService);

        _catalogService = catalogService;
        _cartService = cartService;
        _favoriteService = favoriteService;
    }

    /// <summary>
    /// Идентификатор товара.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Количество при добавлении в корзину.
    /// </summary>
    [BindProperty]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Товар.
    /// </summary>
    public ProductSummary? Product { get; private set; }

    /// <summary>
    /// Количество товара в корзине.
    /// </summary>
    public int QuantityInCart { get; private set; }

    /// <summary>
    /// Признак того, что товар в избранном.
    /// </summary>
    public bool IsFavorite { get; private set; }

    /// <summary>
    /// Загружает карточку товара.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Product = await _catalogService.GetProductAsync(Id, cancellationToken);

        if (Product is null)
        {
            return NotFound();
        }

        QuantityInCart = _cartService.GetQuantity(Product.Id);

        string? userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is not null)
        {
            IsFavorite = (await _favoriteService.GetProductIdsAsync(userId, cancellationToken)).Contains(Product.Id);
        }

        return Page();
    }

    /// <summary>
    /// Добавляет товар в корзину.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление в корзину.</returns>
    public async Task<IActionResult> OnPostAddToCartAsync(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cancellationToken);

        if (Quantity < 1 || Quantity > CartLimits.MaxQuantityPerProduct)
        {
            ModelState.AddModelError(string.Empty, $"Количество должно быть от 1 до {CartLimits.MaxQuantityPerProduct}.");
            Product = await _catalogService.GetProductAsync(Id, cancellationToken);

            return Page();
        }

        try
        {
            _cartService.Add(Id, Quantity);
            TempData["Success"] = "Товар добавлен в корзину.";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage("/Cart/Index");
    }

    /// <summary>
    /// Добавляет или убирает товар из избранного.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на карточку товара.</returns>
    public async Task<IActionResult> OnPostToggleFavoriteAsync(CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            return RedirectToPage("/Account/Login");
        }

        bool added = await _favoriteService.ToggleAsync(userId, Id, cancellationToken);
        TempData[added ? "Success" : "Info"] = added ? "Добавлено в избранное." : "Убрано из избранного.";

        return RedirectToPage();
    }
}