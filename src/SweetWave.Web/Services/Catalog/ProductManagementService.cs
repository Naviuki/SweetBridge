using Microsoft.EntityFrameworkCore;
using SweetWave.Web.Domain.Entities;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Infrastructure.Persistence;

namespace SweetWave.Web.Services.Catalog;

/// <summary>
/// Admin operations over the catalogue.
/// </summary>
public sealed class ProductManagementService : IProductManagementService
{
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductManagementService"/> class.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    public ProductManagementService(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductSummary>> GetAllAsync(CancellationToken cancellationToken) =>
        await _dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Region)
            .ThenBy(product => product.Name)
            .Select(product => ProductSummary.FromProduct(product))
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<ProductSummary> CreateAsync(ProductInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        bool exists = await _dbContext.Products.AnyAsync(product => product.Id == input.Id, cancellationToken);
        if (exists)
        {
            throw new DomainException($"Товар с идентификатором {input.Id} уже существует.");
        }

        Product product = Product.Create(
            input.Id.Trim().ToUpperInvariant(),
            input.Region,
            input.Manufacturer,
            input.Name,
            input.Category,
            input.WeightGrams,
            input.Price,
            input.IsTemperatureSensitive,
            input.ShelfLifeDays,
            input.Photo,
            input.Description);

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ProductSummary.FromProduct(product);
    }

    /// <inheritdoc />
    public async Task<ProductSummary> UpdateAsync(string id, ProductInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        Product product = await GetRequiredProductAsync(id, cancellationToken);

        product.Update(
            input.Manufacturer,
            input.Name,
            input.Category,
            input.WeightGrams,
            input.Price,
            input.IsTemperatureSensitive,
            input.ShelfLifeDays,
            input.Photo,
            input.Description);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ProductSummary.FromProduct(product);
    }

    /// <inheritdoc />
    public async Task SetVisibilityAsync(string id, bool isVisible, CancellationToken cancellationToken)
    {
        Product product = await GetRequiredProductAsync(id, cancellationToken);

        if (isVisible)
        {
            product.Show();
        }
        else
        {
            product.Hide();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Product> GetRequiredProductAsync(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new DomainException("Не указан идентификатор товара.");
        }

        return await _dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken)
            ?? throw new DomainException($"Товар {id} не найден.");
    }
}