using System.ComponentModel.DataAnnotations;

namespace SweetWave.Web.Options;

/// <summary>
/// Shipping discount applied when the wave fill percentage is strictly greater than the threshold.
/// </summary>
public sealed class FillDiscountOption
{
    /// <summary>
    /// Fill percentage threshold, in percent.
    /// </summary>
    [Range(0, 100)]
    public decimal FillAbovePercent { get; set; }

    /// <summary>
    /// Discount applied to the delivery cost, in percent.
    /// </summary>
    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }
}