using System.ComponentModel.DataAnnotations;

namespace SweetWave.Web.Options;

/// <summary>
/// Скидка на доставку, применяемая, когда заполнение волны строго больше порога.
/// </summary>
public sealed class FillDiscountOption
{
    /// <summary>
    /// Порог заполнения волны в процентах.
    /// </summary>
    [Range(0, 100)]
    public decimal FillAbovePercent { get; set; }

    /// <summary>
    /// Скидка на стоимость доставки в процентах.
    /// </summary>
    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }
}
