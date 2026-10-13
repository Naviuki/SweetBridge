using SweetWave.Web.Services.Catalog;

namespace SweetWave.Web.Services.Cart;

/// <summary>
/// Read model of a cart line: the catalogue product and the ordered quantity.
/// </summary>
/// <param name="Product">Catalogue product with the actual price and weight.</param>
/// <param name="Quantity">Ordered quantity.</param>
public sealed record CartLineView(ProductSummary Product, int Quantity)
{
    /// <summary>
    /// Total weight of the line, in grams.
    /// </summary>
    public int LineWeightGrams => Product.WeightGrams * Quantity;

    /// <summary>
    /// Total cost of the line, in rubles.
    /// </summary>
    public decimal LineTotal => Product.Price * Quantity;
}