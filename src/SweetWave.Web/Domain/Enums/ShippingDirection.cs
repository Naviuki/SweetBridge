namespace SweetWave.Web.Domain.Enums;

/// <summary>
/// Направление сборной доставки («волны»): товары всегда едут в противоположный регион.
/// </summary>
public enum ShippingDirection
{
    /// <summary>
    /// Кондитерские изделия из Якутии в ДНР.
    /// </summary>
    YakutiaToDonbass = 0,

    /// <summary>
    /// Кондитерские изделия из Донбасса в Якутию.
    /// </summary>
    DonbassToYakutia = 1
}
