using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Infrastructure.Persistence;

namespace SweetBridge.Web.Services.Catalog;

/// <summary>
/// Операции админки над каталогом.
/// </summary>
public sealed class ProductManagementService : IProductManagementService
{
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="ProductManagementService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public ProductManagementService(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductSummary>> GetAllAsync(CancellationToken cancellationToken) =>
        await _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Producer)
            .OrderBy(product => product.Region)
            .ThenBy(product => product.Name)
            .Select(product => ProductSummary.FromProduct(product))
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<ProductSummary> CreateAsync(ProductInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        string id = input.Id.Trim().ToUpperInvariant();
        bool exists = await _dbContext.Products.AnyAsync(product => product.Id == id, cancellationToken);
        if (exists)
        {
            throw new DomainException($"Товар с идентификатором {id} уже существует.");
        }

        Producer producer = await GetOrCreateProducerAsync(input.ProducerName, input.Region, cancellationToken);

        Product product = Product.Create(
            id,
            producer.Id,
            input.Region,
            input.Name,
            input.Category,
            input.WeightGrams,
            input.Price,
            input.IsTemperatureSensitive,
            input.ShelfLifeDays,
            input.PhotoPath,
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
        Producer producer = await GetOrCreateProducerAsync(input.ProducerName, input.Region, cancellationToken);

        product.Update(
            producer.Id,
            input.Region,
            input.Name,
            input.Category,
            input.WeightGrams,
            input.Price,
            input.IsTemperatureSensitive,
            input.ShelfLifeDays,
            input.PhotoPath,
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

    private async Task<Producer> GetOrCreateProducerAsync(string name, Domain.Enums.Region region, CancellationToken cancellationToken)
    {
        string trimmed = (name ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            throw new DomainException("Укажите производителя товара.");
        }

        Producer? existing = await _dbContext.Producers
            .FirstOrDefaultAsync(producer => producer.Name == trimmed && producer.Region == region, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        Producer created = Producer.Create(trimmed, region, description: null);
        _dbContext.Producers.Add(created);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return created;
    }

    private async Task<Product> GetRequiredProductAsync(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new DomainException("Не указан идентификатор товара.");
        }

        return await _dbContext.Products
                   .Include(product => product.Producer)
                   .FirstOrDefaultAsync(product => product.Id == id, cancellationToken)
               ?? throw new DomainException($"Товар {id} не найден.");
    }
}