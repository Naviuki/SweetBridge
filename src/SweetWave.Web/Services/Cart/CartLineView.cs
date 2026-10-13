using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Cart;

/// <summary>
/// Read model of a cart line.
/// </summary>
/// <param name="ProductId">Product identifier.</param>
/// <param name="Name">Product name.</param>
/// <param name="Manufacturer">Manufacturer name.</param>
/// <param name="Category">Catalogue category.</param>
/// <param name="Region">Region of the manufacturer.</param>
/// <param name="UnitPrice">Price of a single unit, in rubles.</param>
/// <param name="WeightGrams">Weight of a single unit, in grams.</param>
/// <param name="Quantity">Ordered quantity.</param>
/// <param name="IsTemperatureSensitive">Temperature sensitive flag.</param>
/// <param name="Photo">Photo file name or <see langword="null"/>.</param>
/// <param name="ShippingDirection">Delivery direction of the product.</param>
public sealed record CartLineView(
    string ProductId,
    string Name,
    string Manufacturer,
    string Category,
    Region Region,
    decimal UnitPrice,
    int WeightGrams,
    int Quantity,
    bool IsTemperatureSensitive,
    string? Photo,
    ShippingDirection ShippingDirection)
{
    /// <summary>
    /// Total weight of the line, in grams.
    /// </summary>
    public int LineWeightGrams => WeightGrams * Quantity;

    /// <summary>
    /// Total cost of the line, in rubles.
    /// </summary>
    public decimal LineTotal => UnitPrice * Quantity;
}