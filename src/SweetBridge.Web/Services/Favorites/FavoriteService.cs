using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Common;

namespace SweetBridge.Web.Services.Favorites;

/// <summary>
/// Избранные товары пользователя.
/// </summary>
public interface IFavoriteService
{
    /// <summary>
    /// Возвращает список избранного пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Избранные товары.</returns>
    Task<IReadOnlyList<ProductSummary>> GetAllAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает идентификаторы избранных товаров.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Множество идентификаторов товаров.</returns>
    Task<IReadOnlySet<string>> GetProductIdsAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Добавляет или убирает товар из избранного.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns><see langword="true"/>, если товар добавлен, иначе удалён.</returns>
    Task<bool> ToggleAsync(string userId, string productId, CancellationToken cancellationToken);
}

/// <summary>
/// Избранное поверх базы данных.
/// </summary>
public sealed class FavoriteService : IFavoriteService
{
    private readonly AppDbContext _dbContext;
    private readonly IBusinessClock _clock;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="FavoriteService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="clock">Источник времени.</param>
    public FavoriteService(AppDbContext dbContext, IBusinessClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(clock);

        _dbContext = dbContext;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductSummary>> GetAllAsync(string userId, CancellationToken cancellationToken)
    {
        var productIds = await _dbContext.Favorites
            .AsNoTracking()
            .Where(favorite => favorite.UserId == userId)
            .Select(favorite => favorite.ProductId)
            .ToListAsync(cancellationToken);

        if (productIds.Count == 0)
        {
            return [];
        }

        var products = await _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Producer)
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellationToken);

        var byId = products.ToDictionary(product => product.Id, StringComparer.Ordinal);

        return productIds
            .Select(id => byId.GetValueOrDefault(id))
            .Where(product => product is not null)
            .Select(product => ProductSummary.FromProduct(product!))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlySet<string>> GetProductIdsAsync(string userId, CancellationToken cancellationToken)
    {
        var ids = await _dbContext.Favorites
            .AsNoTracking()
            .Where(favorite => favorite.UserId == userId)
            .Select(favorite => favorite.ProductId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet(StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public async Task<bool> ToggleAsync(string userId, string productId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);

        string normalizedId = productId.Trim().ToUpperInvariant();

        bool exists = await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(product => product.Id == normalizedId && product.IsActive, cancellationToken);

        if (!exists)
        {
            return false;
        }

        Favorite? favorite = await _dbContext.Favorites
            .FirstOrDefaultAsync(item => item.UserId == userId && item.ProductId == normalizedId, cancellationToken);

        if (favorite is null)
        {
            _dbContext.Favorites.Add(Favorite.Create(userId, normalizedId, _clock.UtcNow));
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        _dbContext.Favorites.Remove(favorite);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return false;
    }
}