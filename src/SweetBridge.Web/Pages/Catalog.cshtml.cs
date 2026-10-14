using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Cart;
using SweetBridge.Web.Services.Catalog;

namespace SweetBridge.Web.Pages;

/// <summary>
/// Каталог одной витрины с фильтром по категории и поиском по названию.
/// </summary>
/// <param name="catalogService">Сервис каталога.</param>
/// <param name="cartService">Сервис корзины.</param>
public sealed class CatalogModel(ICatalogService catalogService, ICartService cartService) : PageModel
{
    /// <summary>
    /// Витрина, из которой отправляются товары.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public ShippingDirection Direction { get; set; } = ShippingDirection.YakutiaToDonbass;

    /// <summary>
    /// Выбранная категория, пусто — все категории.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    /// <summary>
    /// Поисковый запрос по наименованию товара и производителю.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    [StringLength(100, ErrorMessage = "Слишком длинный поисковый запрос.")]
    public string? SearchText { get; set; }

    /// <summary>
    /// Категории витрины.
    /// </summary>
    public IReadOnlyList<string> Categories { get; private set; } = [];

    /// <summary>
    /// Товары, подходящие под фильтр.
    /// </summary>
    public IReadOnlyList<ProductSummary> Products { get; private set; } = [];

    /// <summary>
    /// Загружает категории и товары витрины.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Categories = await catalogService.GetCategoriesAsync(Direction, cancellationToken);

        Products = await catalogService.GetProductsAsync(
            new CatalogQuery(Direction, Category, SearchText),
            cancellationToken);
    }

    /// <summary>
    /// Добавляет товар в корзину и возвращает в каталог с flash-сообщением.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="quantity">Количество для добавления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Возврат в каталог.</returns>
    public async Task<IActionResult> OnPostAddToCartAsync(string productId, int quantity, CancellationToken cancellationToken)
    {
        try
        {
            await cartService.AddItemAsync(productId, quantity, cancellationToken);
            TempData["Success"] = "Товар добавлен в корзину.";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage(new { Direction, Category, SearchText });
    }
}
