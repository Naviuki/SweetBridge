using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Cart;

/// <summary>
/// Read model of the cart.
/// </summary>
/// <param name="Lines">Cart lines.</param>
public sealed record CartView(IReadOnlyList<CartLineView> Lines)
{
    /// <summary>
    /// Indicates that the cart has no lines.
    /// </summary>
    public bool IsEmpty => Lines.Count == 0;

    /// <summary>
    /// Direction of the showcase the cart belongs to, or <see langword="null"/> for an empty cart.
    /// </summary>
    public ShippingDirection? Direction => Lines.Count == 0 ? null : Lines[0].ShippingDirection;

    /// <summary>
    /// Total number of goods in the cart.
    /// </summary>
    public int TotalQuantity => Lines.Sum(line => line.Quantity);

    /// <summary>
    /// Total weight of the cart, in grams.
    /// </summary>
    public int TotalWeightGrams => Lines.Sum(line => line.LineWeightGrams);

    /// <summary>
    /// Total cost of the goods, in rubles.
    /// </summary>
    public decimal GoodsCost => Lines.Sum(line => line.LineTotal);

    /// <summary>
    /// Indicates that the cart contains temperature sensitive goods.
    /// </summary>
    public bool HasTemperatureSensitiveItems => Lines.Any(line => line.IsTemperatureSensitive);
}