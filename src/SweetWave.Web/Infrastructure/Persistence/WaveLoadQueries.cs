using Microsoft.EntityFrameworkCore;
using SweetWave.Web.Infrastructure.Persistence;

namespace SweetWave.Web.Infrastructure.Persistence;

/// <summary>
/// Single place where the weight loaded into waves is calculated: SQLite sums integer grams,
/// money is never aggregated in SQL.
/// </summary>
public static class WaveLoadQueries
{
    /// <summary>
    /// Returns the weight of the orders of every wave.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <param name="waveIds">Identifiers of the waves.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary with the loaded weight in grams for each wave that has orders.</returns>
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
    /// Returns the weight of the orders of a single wave.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <param name="waveId">Identifier of the wave.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Loaded weight in grams.</returns>
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