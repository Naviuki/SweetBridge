using SweetWave.Web.Services.Catalog;

namespace SweetWave.Web.Services.Seasons;

/// <summary>
/// Seasonal rules: thermal packaging and delivery duration.
/// </summary>
public interface ISeasonPolicy
{
    /// <summary>
    /// Checks whether thermal packaging is mandatory for the given goods and shipment date.
    /// </summary>
    /// <param name="products">Products of the order.</param>
    /// <param name="shipDate">Planned shipment date.</param>
    /// <returns><see langword="true"/> when thermal packaging cannot be declined.</returns>
    bool IsThermoPackRequired(IEnumerable<ProductSummary> products, DateOnly shipDate);

    /// <summary>
    /// Returns the number of days added to the delivery duration in the off-season months.
    /// </summary>
    /// <param name="shipDate">Planned shipment date.</param>
    /// <returns>Number of extra days.</returns>
    int GetExtraDeliveryDays(DateOnly shipDate);
}