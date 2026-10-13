using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Waves;

/// <summary>
/// Data of a wave created in the admin section. Capacity and minimal load come from the configuration.
/// </summary>
/// <param name="Direction">Delivery direction.</param>
/// <param name="ShipDate">Planned shipment date.</param>
public sealed record CreateWaveCommand(ShippingDirection Direction, DateOnly ShipDate);