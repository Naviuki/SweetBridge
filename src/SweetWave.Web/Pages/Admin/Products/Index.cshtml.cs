using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Services.Catalog;

namespace SweetWave.Web.Pages.Admin.Products;

/// <summary>
/// Catalogue management: list of all products including the hidden ones.
/// </summary>
/// <param name="productManagementService">Product management service.</param>
public sealed class IndexModel(IProductManagementService productManagementService) : PageModel
{
    /// <summary>
    /// All products of the catalogue.
    /// </summary>
    public IReadOnlyList<ProductSummary> Products { get; private set; } = [];

    /// <summary>
    /// Loads all products.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Products = await productManagementService.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Hides a product or returns it to the catalogue.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="isVisible">Target visibility.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect back to the list.</returns>
    public async Task<IActionResult> OnPostVisibilityAsync(string id, bool isVisible, CancellationToken cancellationToken)
    {
        try
        {
            await productManagementService.SetVisibilityAsync(id, isVisible, cancellationToken);
            TempData["Success"] = isVisible
                ? $"Товар {id} снова в каталоге."
                : $"Товар {id} скрыт из каталога.";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage();
    }
}