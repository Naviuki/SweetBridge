using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Infrastructure.Persistence;

namespace SweetBridge.Web.Services.Catalog;

/// <summary>
/// Запросы каталога для публичной витрины.
/// </summary>
public sealed class CatalogService : ICatalogService
{
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="CatalogService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
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

        IQueryable<Product> products = _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Producer)
            .Where(product => product.IsActive && product.Region == region);

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            string category = query.Category.Trim();
            products = products.Where(product => product.Category == category);
        }

        if (query.ThermoSensitiveOnly)
        {
            products = products.Where(product => product.IsTemperatureSensitive);
        }

        List<Product> result = await products
            .OrderBy(product => product.Category)
            .ThenBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return FilterByText([.. result.Select(ProductSummary.FromProduct)], query.SearchText);
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

    /// <inheritdoc />
    public async Task<ProductSummary?> GetProductAsync(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        string normalized = id.Trim().ToUpperInvariant();

        return await _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Producer)
            .FirstOrDefaultAsync(product => product.Id == normalized && product.IsActive, cancellationToken)
            is Product product
            ? ProductSummary.FromProduct(product)
            : null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductSummary>> GetShowcaseAsync(
        ShippingDirection direction,
        int count,
        CancellationToken cancellationToken)
    {
        Region region = ToRegion(direction);

        return await _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Producer)
            .Where(product => product.IsActive && product.Region == region)
            .OrderBy(product => product.Category)
            .ThenBy(product => product.Name)
            .Take(count)
            .Select(product => ProductSummary.FromProduct(product))
            .ToListAsync(cancellationToken);
    }

    /// <remarks>
    /// SQLite сравнивает строки с кириллицей регистрозависимо, поэтому текстовый поиск выполняется в памяти
    /// по небольшому каталогу витрины.
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
                || product.ProducerName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || product.Category.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }
}