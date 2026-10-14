using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Cart;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Favorites;
using SweetBridge.Web.Services.Waves;

namespace SweetBridge.Web.Pages.Catalog;

/// <summary>
/// Каталог витрины с фильтрами, поиском, постраничным выводом и подгрузкой товаров.
/// </summary>
/// <param name="catalogService">Сервис каталога.</param>
/// <param name="cartService">Сервис корзины.</param>
/// <param name="favoriteService">Сервис избранного.</param>
/// <param name="waveService">Сервис волн.</param>
public class IndexModel(
    ICatalogService catalogService,
    ICartService cartService,
    IFavoriteService favoriteService,
    IWaveService waveService)
    : PageModel
{
    private const int PageSize = 9;

    /// <summary>
    /// Витрина, из которой отправляются товары.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "direction")]
    public ShippingDirection Direction { get; set; } = ShippingDirection.YakutiaToDonbass;

    /// <summary>
    /// Выбранная категория.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "category")]
    public string? Category { get; set; }

    /// <summary>
    /// Поисковый запрос.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    [StringLength(100)]
    [FromQuery(Name = "q")]
    public string? Search { get; set; }

    /// <summary>
    /// Только термочувствительные товары.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "thermo")]
    public bool ThermoSensitive { get; set; }

    /// <summary>
    /// Номер страницы.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "page")]
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Выбранная волна: используется для подсказки в корзине.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "wave")]
    public int? Wave { get; set; }

    /// <summary>
    /// Товары текущей страницы.
    /// </summary>
    public IReadOnlyList<ProductCardViewModel> Products { get; private set; } = [];

    /// <summary>
    /// Категории витрины.
    /// </summary>
    public IReadOnlyList<string> Categories { get; private set; } = [];

    /// <summary>
    /// Общее количество товаров по фильтру.
    /// </summary>
    public int TotalItems { get; private set; }

    /// <summary>
    /// Номер следующей страницы или <see langword="null"/>, если товары закончились.
    /// </summary>
    public int? NextPage { get; private set; }

    /// <summary>
    /// Идентификаторы избранных товаров.
    /// </summary>
    public IReadOnlySet<string> FavoriteIds { get; private set; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Признак того, что хотя бы один фильтр активен.
    /// </summary>
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Category) || !string.IsNullOrWhiteSpace(Search) || ThermoSensitive || CurrentPage > 1;

    /// <summary>
    /// Выбранная волна для плашки в каталоге.
    /// </summary>
    public WaveSummary? SelectedWave { get; private set; }

    /// <summary>
    /// Загружает страницу каталога.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        return Page();
    }

    /// <summary>
    /// Отдаёт следующую порцию карточек для подгрузки без полной перерисовки страницы.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Разметка карточек.</returns>
    public async Task<PartialViewResult> OnGetItemsAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        Response.Headers.CacheControl = "no-store";

        return Partial("_ProductItems", this);
    }

    /// <summary>
    /// Добавляет товар в корзину и возвращает покупателя в каталог.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="quantity">Количество.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление в каталог с теми же фильтрами.</returns>
    public IActionResult OnPostAddToCartAsync(string productId, int quantity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cancellationToken);

        if (quantity < 1 || quantity > CartLimits.MaxQuantityPerProduct)
        {
            TempData["Error"] = $"Количество должно быть от 1 до {CartLimits.MaxQuantityPerProduct}.";

            return LocalRedirect(BuildQueryUrl(1, handler: null));
        }

        try
        {
            cartService.Add(productId, quantity);
            TempData["Success"] = "Товар добавлен в корзину.";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return LocalRedirect(BuildQueryUrl(1, handler: null));
    }

    /// <summary>
    /// Добавляет или убирает товар из избранного.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на текущую страницу каталога.</returns>
    public async Task<IActionResult> OnPostToggleFavoriteAsync(string productId, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Catalog/Index") });
        }

        bool added = await favoriteService.ToggleAsync(userId, productId, cancellationToken);
        TempData[added ? "Success" : "Info"] = added ? "Товар добавлен в избранное." : "Товар убран из избранного.";

        return LocalRedirect(BuildQueryUrl(1, handler: null));
    }

    /// <summary>
    /// Ссылка на следующую страницу с сохранением фильтров.
    /// </summary>
    /// <param name="page">Номер страницы.</param>
    /// <returns>Адрес страницы каталога.</returns>
    public string BuildUrl(int page) => BuildQueryUrl(page, handler: null);

    /// <summary>
    /// Ключ «page» зарезервирован генератором ссылок Razor Pages, поэтому строка запроса
    /// собирается явно: так фильтры и номер страницы гарантированно попадают в адрес.
    /// </summary>
    private string BuildQueryUrl(int page, string? handler)
    {
        string baseUrl = Url.Page("/Catalog/Index") ?? "/Catalog";

        Dictionary<string, string?> query = new(StringComparer.Ordinal)
        {
            ["handler"] = handler,
            ["direction"] = Direction.ToString(),
            ["category"] = Category,
            ["q"] = Search,
            ["thermo"] = ThermoSensitive ? "true" : null,
            ["wave"] = Wave?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["page"] = page > 1 ? page.ToString(System.Globalization.CultureInfo.InvariantCulture) : null
        };

        return QueryHelpers.AddQueryString(baseUrl, query);
    }
    /// <summary>
    /// Ссылка на partial следующей порции товаров.
    /// </summary>
    /// <param name="page">Номер порции.</param>
    /// <returns>Адрес обработчика Items с фильтрами.</returns>
    public string BuildItemsUrl(int page) => BuildQueryUrl(page, handler: "Items");

    /// <summary>
    /// Регион витрины по направлению доставки.
    /// </summary>
    /// <param name="direction">Направление доставки.</param>
    /// <returns>Регион витрины.</returns>
    public static Region ToShowcaseRegion(ShippingDirection direction) =>
        direction == ShippingDirection.YakutiaToDonbass ? Region.Yakutia : Region.Donbass;

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        CatalogQuery query = new(Direction, Category, Search, ThermoSensitive);
        IReadOnlyList<ProductSummary> all = await catalogService.GetProductsAsync(query, cancellationToken);

        Categories = await catalogService.GetCategoriesAsync(Direction, cancellationToken);

        TotalItems = all.Count;
        int totalPages = Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
        CurrentPage = Math.Clamp(CurrentPage, 1, totalPages);
        NextPage = CurrentPage < totalPages ? CurrentPage + 1 : null;

        string? userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        FavoriteIds = userId is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : await favoriteService.GetProductIdsAsync(userId, cancellationToken);

        Products = ToCards(all.Skip((CurrentPage - 1) * PageSize).Take(PageSize));

        if (Wave is not null)
        {
            IReadOnlyList<WaveSummary> waves = await waveService.GetUpcomingAsync(Direction, 6, cancellationToken);
            SelectedWave = waves.FirstOrDefault(wave => wave.Id == Wave);
        }
    }

    private List<ProductCardViewModel> ToCards(IEnumerable<ProductSummary> products) =>
    [
        .. products.Select(product => new ProductCardViewModel(
            product.Id,
            product.Name,
            product.Category,
            product.ProducerName,
            ToShowcaseRegion(product.ShippingDirection),
            product.WeightGrams,
            product.Price,
            product.IsTemperatureSensitive,
            product.PhotoPath,
            cartService.GetQuantity(product.Id),
            FavoriteIds.Contains(product.Id)))
    ];

}
