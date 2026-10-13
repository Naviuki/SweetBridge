using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Catalog;

/// <summary>
/// Read access to the catalogue.
/// </summary>
public interface ICatalogService
{
    /// <summary>
    /// Returns the active products of one showcase that match the filter.
    /// </summary>
    /// <param name="query">Showcase, category and search text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching products.</returns>
    Task<IReadOnlyList<ProductSummary>> GetProductsAsync(CatalogQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the categories that are used by the products of the showcase.
    /// </summary>
    /// <param name="direction">Showcase the products travel from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Category names in alphabetical order.</returns>
    Task<IReadOnlyList<string>> GetCategoriesAsync(ShippingDirection direction, CancellationToken cancellationToken);
}