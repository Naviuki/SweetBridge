using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Services.Catalog;

namespace SweetWave.Web.Services.Waves;

/// <summary>
/// Waves: open offers for the checkout page and administration of the wave schedule.
/// </summary>
public interface IWaveService
{
    /// <summary>
    /// Calculates the offers of all waves that still accept orders in the given direction.
    /// </summary>
    /// <param name="direction">Delivery direction of the cart.</param>
    /// <param name="products">Products of the cart.</param>
    /// <param name="orderWeightGrams">Weight of the cart, in grams.</param>
    /// <param name="thermoPackRequested">Thermal packaging chosen by the buyer.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Offers sorted by the shipment date.</returns>
    Task<IReadOnlyList<WaveOffer>> GetOpenOffersAsync(
        ShippingDirection direction,
        IReadOnlyList<ProductSummary> products,
        int orderWeightGrams,
        bool thermoPackRequested,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the nearest waves that still accept orders, without delivery calculation.
    /// </summary>
    /// <param name="direction">Delivery direction.</param>
    /// <param name="count">Maximum number of waves to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Nearest waves sorted by the shipment date.</returns>
    Task<IReadOnlyList<WaveSummary>> GetUpcomingAsync(
        ShippingDirection direction,
        int count,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns all waves with their fill for the admin list.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All waves sorted by shipment date.</returns>
    Task<IReadOnlyList<WaveSummary>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a planned wave: the order cutoff date and the load limits come from the configuration.
    /// </summary>
    /// <param name="command">Direction and shipment date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created wave.</returns>
    Task<WaveSummary> CreateAsync(CreateWaveCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Dispatches the wave and moves all its orders to the in transit status.
    /// </summary>
    /// <param name="waveId">Wave identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
    Task DispatchAsync(int waveId, CancellationToken cancellationToken);
}