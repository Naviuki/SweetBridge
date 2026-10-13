namespace SweetWave.Web.Domain.Enums;

/// <summary>
/// Direction of a consolidated delivery ("wave"): products always travel to the opposite region.
/// </summary>
public enum ShippingDirection
{
    /// <summary>
    /// Confectionery from Yakutia to Donbass.
    /// </summary>
    YakutiaToDonbass = 0,

    /// <summary>
    /// Confectionery from Donbass to Yakutia.
    /// </summary>
    DonbassToYakutia = 1
}