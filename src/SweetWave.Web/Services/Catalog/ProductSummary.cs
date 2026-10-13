using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Catalog;

/// <summary>
/// Read model of a catalogue product.
/// </summary>
/// <param name="Id">Product identifier.</param>
/// <param name="Region">Region of the manufacturer.</param>
/// <param name="Manufacturer">Manufacturer name.</param>
/// <param name="Name">Product name.</param>
/// <param name="Category">Catalogue category.</param>
/// <param name="WeightGrams">Weight of a single unit, in grams.</param>
/// <param name="Price">Price of a single unit, in rubles.</param>
/// <param name="IsTemperatureSensitive">Temperature sensitive flag.</param>
/// <param name="ShelfLifeDays">Shelf life, in days.</param>
/// <param name="Photo">Photo file name or <see langword="null"/>.</param>
/// <param name="Description">Product description or <see langword="null"/>.</param>
/// <param name="ShippingDirection">Delivery direction of the product.</param>
public sealed record ProductSummary(
    string Id,
    Region Region,
    string Manufacturer,
    string Name,
    string Category,
    int WeightGrams,
    decimal Price,
    bool IsTemperatureSensitive,
    int ShelfLifeDays,
    string? Photo,
    string? Description,
    ShippingDirection ShippingDirection)
{
    /// <summary>
    /// Creates a read model from a domain product.
    /// </summary>
    /// <param name="product">Domain product.</param>
    /// <returns>Read model of the product.</returns>
    public static ProductSummary FromProduct(Domain.Entities.Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductSummary(
            product.Id,
            product.Region,
            product.Manufacturer,
            product.Name,
            product.Category,
            product.WeightGrams,
            product.Price,
            product.IsTemperatureSensitive,
            product.ShelfLifeDays,
            product.Photo,
            product.Description,
            product.ShippingDirection);
    }
}