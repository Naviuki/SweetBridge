using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Waves;

/// <summary>
/// Read model of a wave for the admin list.
/// </summary>
/// <param name="WaveId">Wave identifier.</param>
/// <param name="Direction">Delivery direction.</param>
/// <param name="ShipDate">Planned shipment date.</param>
/// <param name="OrderCutoffDate">Last day when orders are accepted.</param>
/// <param name="CapacityGrams">Maximal weight of the wave, in grams.</param>
/// <param name="MinLoadGrams">Minimal weight required to dispatch, in grams.</param>
/// <param name="LoadedGrams">Weight already loaded into the wave, in grams.</param>
/// <param name="FillPercent">Fill percentage of the wave, in percent.</param>
/// <param name="Status">Wave status.</param>
/// <param name="IsReadyToDispatch">Indicates that the wave holds the minimal load and can be dispatched.</param>
public sealed record WaveSummary(
    int WaveId,
    ShippingDirection Direction,
    DateOnly ShipDate,
    DateOnly OrderCutoffDate,
    int CapacityGrams,
    int MinLoadGrams,
    int LoadedGrams,
    decimal FillPercent,
    WaveStatus Status,
    bool IsReadyToDispatch);