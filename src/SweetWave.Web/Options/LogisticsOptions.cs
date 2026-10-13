using System.ComponentModel.DataAnnotations;

namespace SweetWave.Web.Options;

/// <summary>
/// Logistics parameters of the platform. All delivery numbers come from configuration.
/// </summary>
public sealed class LogisticsOptions
{
    /// <summary>
    /// Name of the configuration section.
    /// </summary>
    public const string SectionName = "Logistics";

    /// <summary>
    /// Base tariff for one kilogram of a parcel, in rubles.
    /// </summary>
    [Range(1, 100_000)]
    public decimal BaseTariffPerKg { get; set; }

    /// <summary>
    /// Cost of thermal packaging, in rubles.
    /// </summary>
    [Range(0, 100_000)]
    public decimal ThermoPackCost { get; set; }

    /// <summary>
    /// Interval between waves of the same direction, in days.
    /// </summary>
    [Range(1, 365)]
    public int WaveIntervalDays { get; set; }

    /// <summary>
    /// How many days before the shipment date orders stop being accepted.
    /// </summary>
    [Range(0, 60)]
    public int OrderCutoffDaysBeforeShipment { get; set; }

    /// <summary>
    /// Minimal total weight of a wave that can be dispatched, in kilograms.
    /// </summary>
    [Range(1, 100_000)]
    public int MinWaveLoadKg { get; set; }

    /// <summary>
    /// Maximal total weight of a wave, in kilograms.
    /// </summary>
    [Range(1, 1_000_000)]
    public int MaxWaveLoadKg { get; set; }

    /// <summary>
    /// Minimal delivery duration, in days.
    /// </summary>
    [Range(1, 365)]
    public int MinDeliveryDays { get; set; }

    /// <summary>
    /// Maximal delivery duration, in days.
    /// </summary>
    [Range(1, 365)]
    public int MaxDeliveryDays { get; set; }

    /// <summary>
    /// Additional days added to the delivery duration during the off-season months.
    /// </summary>
    [Range(0, 365)]
    public int OffSeasonExtraDays { get; set; }

    /// <summary>
    /// Months (1-12) with a longer delivery duration.
    /// </summary>
    [MinLength(1)]
    public List<int> OffSeasonMonths { get; set; } = [];

    /// <summary>
    /// Months (1-12) when thermal packaging is mandatory for temperature sensitive goods.
    /// </summary>
    [MinLength(1)]
    public List<int> ThermoPackMandatoryMonths { get; set; } = [];

    /// <summary>
    /// Discount thresholds by wave fill percentage.
    /// </summary>
    [MinLength(1)]
    public List<FillDiscountOption> FillDiscounts { get; set; } = [];
}