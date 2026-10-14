using SweetBridge.Web.Domain.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Cart;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Favorites;

namespace SweetBridge.Web.Pages.Catalog;

/// <summary>
/// Каталог витрины с фильтрами, поиском и постраничным выводом.
/// </summary>
public class IndexModel : PageModel
{
    private const int PageSize = 9;

    private readonly ICatalogService _catalogService;
    private readonly ICartService _cartService;
    private readonly IFavoriteService _favoriteService;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="IndexModel"/>.
    /// </summary>
    /// <param name="catalogService">Сервис каталога.</param>
    /// <param name="cartService">Сервис корзины.</param>
    /// <param name="favoriteService">Сервис избранного.</param>
    public IndexModel(ICatalogService catalogService, ICartService cartService, IFavoriteService favoriteService)
    {
        ArgumentNullException.ThrowIfNull(catalogService);
        ArgumentNullException.ThrowIfNull(cartService);
        ArgumentNullException.ThrowIfNull(favoriteService);

        _catalogService = catalogService;
        _cartService = cartService;
        _favoriteService = favoriteService;
    }

    /// <summary>
    /// Витрина, из которой отправляются товары.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public ShippingDirection Direction { get; set; } = ShippingDirection.YakutiaToDonbass;

    /// <summary>
    /// Выбранная категория.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    /// <summary>
    /// Поисковый запрос.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    [StringLength(100)]
    public string? Search { get; set; }

    /// <summary>
    /// Только термочувствительные товары.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public bool ThermoSensitive { get; set; }

    /// <summary>
    /// Номер страницы.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Товары текущей страницы.
    /// </summary>
    public IReadOnlyList<ProductCardViewModel> Products { get; private set; } = [];

    /// <summary>
    /// Категории витрины.
    /// </summary>
    public IReadOnlyList<string> Categories { get; private set; } = [];

    /// <summary>
    /// Навигация по страницам.
    /// </summary>
    public PagerViewModel Pager { get; private set; } = new() { UrlFactory = _ => null };

    /// <summary>
    /// Идентификаторы избранных товаров.
    /// </summary>
    public IReadOnlySet<string> FavoriteIds { get; private set; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Загружает страницу каталога.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        CatalogQuery query = new(Direction, Category, Search, ThermoSensitive);
        IReadOnlyList<ProductSummary> all = await _catalogService.GetProductsAsync(query, cancellationToken);

        Categories = await _catalogService.GetCategoriesAsync(Direction, cancellationToken);

        int totalPages = Math.Max(1, (int)Math.Ceiling(all.Count / (double)PageSize));
        CurrentPage = Math.Clamp(CurrentPage, 1, totalPages);

        Pager = new PagerViewModel
        {
            Page = CurrentPage,
            TotalPages = totalPages,
            TotalItems = all.Count,
            UrlFactory = BuildPageUrl
        };

        string? userId = User.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
        FavoriteIds = userId is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : await _favoriteService.GetProductIdsAsync(userId, cancellationToken);

        IReadOnlyList<ProductCardViewModel> pageItems = all
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(product => new ProductCardViewModel(
                product.Id,
                product.Name,
                product.Category,
                product.ProducerName,
                ToShowcaseRegion(product.ShippingDirection),
                product.WeightGrams,
                product.Price,
                product.IsTemperatureSensitive,
                product.PhotoPath,
                _cartService.GetQuantity(product.Id),
                FavoriteIds.Contains(product.Id)))
            .ToList();

        Products = pageItems;
    }

    /// <summary>
    /// Добавляет товар в корзину и возвращает покупателя в каталог.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="quantity">Количество.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на каталог.</returns>
    public async Task<IActionResult> OnPostAddToCartAsync(
        string productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cancellationToken);

        if (quantity < 1 || quantity > CartLimits.MaxQuantityPerProduct)
        {
            ModelState.AddModelError(string.Empty, $"Количество должно быть от 1 до {CartLimits.MaxQuantityPerProduct}.");

            await OnGetAsync(cancellationToken);

            return Page();
        }

        try
        {
            _cartService.Add(productId, quantity);
            TempData["Success"] = "Товар добавлен в корзину.";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Добавляет или убирает товар из избранного.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на текущую страницу каталога.</returns>
    public async Task<IActionResult> OnPostToggleFavoriteAsync(
        string productId,
        CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            return RedirectToPage("/Account/Login");
        }

        bool added = await _favoriteService.ToggleAsync(userId, productId, cancellationToken);
        TempData[added ? "Success" : "Info"] = added
            ? "Товар добавлен в избранное."
            : "Товар убран из избранного.";

        return RedirectToPage();
    }

    /// <summary>
    /// Регион витрины по направлению доставки.
    /// </summary>
    /// <param name="direction">Направление доставки.</param>
    /// <returns>Регион витрины.</returns>
    public static Region ToShowcaseRegion(ShippingDirection direction) =>
        direction == ShippingDirection.YakutiaToDonbass ? Region.Yakutia : Region.Donbass;

    private string? BuildPageUrl(int page)
    {
        Dictionary<string, string?> parameters = new(StringComparer.Ordinal)
        {
            ["region"] = Direction.ToString(),
            ["category"] = Category,
            ["q"] = Search,
            ["thermo"] = ThermoSensitive ? "true" : null,
            ["page"] = page > 1 ? page.ToString(System.Globalization.CultureInfo.InvariantCulture) : null
        };

        return Url.Page("/Catalog/Index", values: parameters);
    }
}