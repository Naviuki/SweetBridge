using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Catalog;

/// <summary>
/// Filter of the catalogue: showcase, category and free text search.
/// </summary>
/// <param name="Direction">Showcase the products travel from.</param>
/// <param name="Category">Category to filter, or <see langword="null"/> for all categories.</param>
/// <param name="SearchText">Search text for the name and the manufacturer.</param>
public sealed record CatalogQuery(ShippingDirection Direction, string? Category = null, string? SearchText = null);