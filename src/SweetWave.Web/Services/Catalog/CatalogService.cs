using Microsoft.EntityFrameworkCore;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Infrastructure.Persistence;

namespace SweetWave.Web.Services.Catalog;

/// <summary>
/// Catalogue queries for the public showcase.
/// </summary>
public sealed class CatalogService : ICatalogService
{
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CatalogService"/> class.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    public CatalogService(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductSummary>> GetProductsAsync(CatalogQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        Region region = ToRegion(query.Direction);

        IQueryable<ProductSummary> products = _dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive && product.Region == region)
            .Select(product => new ProductSummary(
                product.Id,
                product.Region,
                product.Manufacturer,
                product.Name,
                product.Category,
                product.WeightGrams,
                product.Price,
                product.IsTemperatureSensitive,
                product.ShelfLifeDays,
                product.Photo,
                product.Description,
                product.ShippingDirection));

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            string category = query.Category.Trim();
            products = products.Where(product => product.Category == category);
        }

        List<ProductSummary> result = await products
            .OrderBy(product => product.Category)
            .ThenBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return FilterByText(result, query.SearchText);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetCategoriesAsync(ShippingDirection direction, CancellationToken cancellationToken)
    {
        Region region = ToRegion(direction);

        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive && product.Region == region)
            .Select(product => product.Category)
            .Distinct()
            .OrderBy(category => category)
            .ToListAsync(cancellationToken);
    }

    /// <remarks>
    /// SQLite comparisons are case sensitive for Cyrillic, so the free text search runs in memory
    /// over the small catalogue of the showcase.
    /// </remarks>
    private static Region ToRegion(ShippingDirection direction) =>
        direction == ShippingDirection.YakutiaToDonbass ? Region.Yakutia : Region.Donbass;

    private static List<ProductSummary> FilterByText(List<ProductSummary> products, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return products;
        }

        string[] terms = searchText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return products
            .Where(product => terms.All(term =>
                product.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || product.Manufacturer.Contains(term, StringComparison.OrdinalIgnoreCase)
                || product.Category.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }
}