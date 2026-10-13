using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Catalog;

/// <summary>
/// Data of a product created or edited in the admin section.
/// </summary>
/// <param name="Id">Product identifier used on creation and ignored on editing.</param>
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
public sealed record ProductInput(
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
    string? Description);