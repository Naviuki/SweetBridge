using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Services.Cart;
using SweetWave.Web.Services.Catalog;

namespace SweetWave.Web.Pages;

/// <summary>
/// Catalogue of one showcase with the category filter and the search by name.
/// </summary>
/// <param name="catalogService">Catalogue service.</param>
/// <param name="cartService">Cart service.</param>
public sealed class CatalogModel(ICatalogService catalogService, ICartService cartService) : PageModel
{
    /// <summary>
    /// Showcase the products travel from.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public ShippingDirection Direction { get; set; } = ShippingDirection.YakutiaToDonbass;

    /// <summary>
    /// Selected category, empty for all categories.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    /// <summary>
    /// Search text for the product name and manufacturer.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    [StringLength(100, ErrorMessage = "Слишком длинный поисковый запрос.")]
    public string? SearchText { get; set; }

    /// <summary>
    /// Categories of the showcase.
    /// </summary>
    public IReadOnlyList<string> Categories { get; private set; } = [];

    /// <summary>
    /// Products that match the filter.
    /// </summary>
    public IReadOnlyList<ProductSummary> Products { get; private set; } = [];

    /// <summary>
    /// Loads the categories and the products of the showcase.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Categories = await catalogService.GetCategoriesAsync(Direction, cancellationToken);

        Products = await catalogService.GetProductsAsync(
            new CatalogQuery(Direction, Category, SearchText),
            cancellationToken);
    }

    /// <summary>
    /// Adds a product to the cart and returns to the catalogue with a flash message.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="quantity">Quantity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect back to the catalogue.</returns>
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