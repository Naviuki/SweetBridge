using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Infrastructure.Persistence;

namespace SweetBridge.Web.Infrastructure.Persistence;

/// <summary>
/// Единственное место, где считается загруженный вес волн: SQLite суммирует целые граммы,
/// а деньги в SQL не агрегируются.
/// </summary>
public static class WaveLoadQueries
{
    /// <summary>
    /// Возвращает вес заказов каждой волны.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="waveIds">Идентификаторы волн.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Словарь с загруженным весом в граммах для каждой волны, где есть заказы.</returns>
    public static async Task<Dictionary<int, int>> GetLoadedGramsAsync(
        AppDbContext dbContext,
        IReadOnlyCollection<int> waveIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(waveIds);

        if (waveIds.Count == 0)
        {
            return [];
        }

        int[] ids = [.. waveIds];

        return await (from item in dbContext.OrderItems.AsNoTracking()
                      join order in dbContext.Orders.AsNoTracking() on item.OrderId equals order.Id
                      where ids.Contains(order.WaveId)
                      group item by order.WaveId into loaded
                      select new
                      {
                          WaveId = loaded.Key,
                          Grams = loaded.Sum(position => position.WeightGrams * position.Quantity)
                      })
            .ToDictionaryAsync(item => item.WaveId, item => item.Grams, cancellationToken);
    }

    /// <summary>
    /// Возвращает вес заказов одной волны.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="waveId">Идентификатор волны.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Загруженный вес в граммах.</returns>
    public static async Task<int> GetLoadedGramsAsync(
        AppDbContext dbContext,
        int waveId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        Dictionary<int, int> loadedGrams = await GetLoadedGramsAsync(dbContext, [waveId], cancellationToken);

        return loadedGrams.GetValueOrDefault(waveId);
    }
}
