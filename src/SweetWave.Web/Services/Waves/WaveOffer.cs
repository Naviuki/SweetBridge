using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Services.Delivery;

namespace SweetWave.Web.Services.Waves;

/// <summary>
/// Offer of a wave for the current cart: dates, fill, delivery cost and delivery period.
/// </summary>
/// <param name="WaveId">Wave identifier.</param>
/// <param name="Direction">Delivery direction.</param>
/// <param name="ShipDate">Planned shipment date.</param>
/// <param name="OrderCutoffDate">Last day when orders are accepted.</param>
/// <param name="CapacityGrams">Maximal weight of the wave, in grams.</param>
/// <param name="LoadedGrams">Weight already loaded into the wave, in grams.</param>
/// <param name="FillPercentBefore">Fill percentage before the order, in percent.</param>
/// <param name="FillPercentAfter">Fill percentage after the order, in percent.</param>
/// <param name="CanAcceptOrder">Indicates that the order fits into the remaining capacity.</param>
/// <param name="DeliveryCost">Calculated delivery cost.</param>
/// <param name="ThermoPackRequired">Indicates that thermal packaging cannot be declined.</param>
/// <param name="DeliveryDaysMin">Minimal delivery duration, in days.</param>
/// <param name="DeliveryDaysMax">Maximal delivery duration, in days.</param>
/// <param name="EstimatedDeliveryFrom">Earliest estimated delivery date.</param>
/// <param name="EstimatedDeliveryTo">Latest estimated delivery date.</param>
public sealed record WaveOffer(
    int WaveId,
    ShippingDirection Direction,
    DateOnly ShipDate,
    DateOnly OrderCutoffDate,
    int CapacityGrams,
    int LoadedGrams,
    decimal FillPercentBefore,
    decimal FillPercentAfter,
    bool CanAcceptOrder,
    DeliveryCostResult DeliveryCost,
    bool ThermoPackRequired,
    int DeliveryDaysMin,
    int DeliveryDaysMax,
    DateOnly EstimatedDeliveryFrom,
    DateOnly EstimatedDeliveryTo)
{
    /// <summary>
    /// Free capacity of the wave after the order, in grams.
    /// </summary>
    public int RemainingGrams => CapacityGrams - LoadedGrams;
}