using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Services.Cart;
using Microsoft.EntityFrameworkCore;

namespace SweetBridge.Web.Services.Gifts;

/// <summary>
/// Модель чтения позиции подарочного набора.
/// </summary>
/// <param name="ProductId">Идентификатор товара.</param>
/// <param name="ProductName">Наименование товара.</param>
/// <param name="Quantity">Количество.</param>
public sealed record GiftSetItemSummary(string ProductId, string ProductName, int Quantity);

/// <summary>
/// Модель чтения подарочного набора.
/// </summary>
/// <param name="Id">Идентификатор набора.</param>
/// <param name="Name">Название.</param>
/// <param name="Showcase">Витрина.</param>
/// <param name="Description">Описание.</param>
/// <param name="PhotoPath">Путь к фотографии.</param>
/// <param name="Price">Цена.</param>
/// <param name="TotalWeightGrams">Суммарный вес.</param>
/// <param name="Items">Состав набора.</param>
public sealed record GiftSetSummary(
    int Id,
    string Name,
    Region Showcase,
    string Description,
    string? PhotoPath,
    decimal Price,
    int TotalWeightGrams,
    IReadOnlyList<GiftSetItemSummary> Items);

/// <summary>
/// Доступ к подарочным наборам.
/// </summary>
public interface IGiftSetService
{
    /// <summary>
    /// Возвращает активные наборы обеих витрин.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Подарочные наборы.</returns>
    Task<IReadOnlyList<GiftSetSummary>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Находит набор по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор набора.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Набор или <see langword="null"/>.</returns>
    Task<GiftSetSummary?> GetAsync(int id, CancellationToken cancellationToken);
}

/// <summary>
/// Подарочные наборы поверх базы данных.
/// </summary>
public sealed class GiftSetService : IGiftSetService
{
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="GiftSetService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public GiftSetService(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GiftSetSummary>> GetAllAsync(CancellationToken cancellationToken)
    {
        var sets = await _dbContext.GiftSets
            .AsNoTracking()
            .Include(set => set.Items)
            .Where(set => set.IsActive)
            .OrderBy(set => set.Showcase)
            .ThenBy(set => set.Name)
            .ToListAsync(cancellationToken);

        return [.. sets.Select(ToSummary)];
    }

    /// <inheritdoc />
    public async Task<GiftSetSummary?> GetAsync(int id, CancellationToken cancellationToken)
    {
        Domain.Entities.GiftSet? set = await _dbContext.GiftSets
            .AsNoTracking()
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive, cancellationToken);

        return set is null ? null : ToSummary(set);
    }

    private static GiftSetSummary ToSummary(Domain.Entities.GiftSet set) =>
        new(
            set.Id,
            set.Name,
            set.Showcase,
            set.Description,
            set.PhotoPath,
            set.Price,
            set.TotalWeightGrams,
            [.. set.Items.Select(item => new GiftSetItemSummary(item.ProductId, item.ProductName, item.Quantity))]);
}