using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;

namespace SweetWave.Web.Domain.Entities;

/// <summary>
/// A confectionery product of one of the two showcases.
/// </summary>
public sealed class Product
{
    private Product()
    {
    }

    private Product(
        string id,
        Region region,
        string manufacturer,
        string name,
        string category,
        int weightGrams,
        decimal price,
        bool isTemperatureSensitive,
        int shelfLifeDays,
        string? photoFileName,
        string? description)
    {
        Id = id;
        Region = region;
        Manufacturer = manufacturer;
        Name = name;
        Category = category;
        WeightGrams = weightGrams;
        Price = price;
        IsTemperatureSensitive = isTemperatureSensitive;
        ShelfLifeDays = shelfLifeDays;
        PhotoFileName = photoFileName;
        Description = description;
        IsActive = true;
    }

    /// <summary>
    /// Product identifier in the catalogue format, for example <c>YKT-001</c>.
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Region of the manufacturer.
    /// </summary>
    public Region Region { get; private set; }

    /// <summary>
    /// Manufacturer name.
    /// </summary>
    public string Manufacturer { get; private set; } = string.Empty;

    /// <summary>
    /// Product name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Catalogue category used by the filter.
    /// </summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// Weight of a single unit, in grams.
    /// </summary>
    public int WeightGrams { get; private set; }

    /// <summary>
    /// Price of a single unit, in rubles.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Indicates that the product cannot be shipped without thermal packaging in cold and hot months.
    /// </summary>
    public bool IsTemperatureSensitive { get; private set; }

    /// <summary>
    /// Shelf life, in days.
    /// </summary>
    public int ShelfLifeDays { get; private set; }

    /// <summary>
    /// Photo file name inside <c>wwwroot/images</c>, or <see langword="null"/> to use the placeholder.
    /// </summary>
    public string? PhotoFileName { get; private set; }

    /// <summary>
    /// Product description shown on the card.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Soft deletion flag: hidden products stay in past orders but disappear from the catalogue.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Delivery direction: goods of a region always travel to the opposite region.
    /// </summary>
    public ShippingDirection ShippingDirection => Region == Region.Yakutia
        ? ShippingDirection.YakutiaToDonbass
        : ShippingDirection.DonbassToYakutia;

    /// <summary>
    /// Creates a new active product.
    /// </summary>
    /// <param name="id">Unique product identifier.</param>
    /// <param name="region">Region of the manufacturer.</param>
    /// <param name="manufacturer">Manufacturer name.</param>
    /// <param name="name">Product name.</param>
    /// <param name="category">Catalogue category.</param>
    /// <param name="weightGrams">Weight of a single unit, in grams.</param>
    /// <param name="price">Price of a single unit, in rubles.</param>
    /// <param name="isTemperatureSensitive">Temperature sensitive flag.</param>
    /// <param name="shelfLifeDays">Shelf life, in days.</param>
    /// <param name="photoFileName">Photo file name or <see langword="null"/>.</param>
    /// <param name="description">Product description or <see langword="null"/>.</param>
    /// <returns>Created product.</returns>
    /// <exception cref="DomainException">Product data is not valid.</exception>
    public static Product Create(
        string id,
        Region region,
        string manufacturer,
        string name,
        string category,
        int weightGrams,
        decimal price,
        bool isTemperatureSensitive,
        int shelfLifeDays,
        string? photoFileName,
        string? description)
    {
        Validate(id, manufacturer, name, category, weightGrams, price, shelfLifeDays);

        return new Product(
            id,
            region,
            manufacturer.Trim(),
            name.Trim(),
            category.Trim(),
            weightGrams,
            price,
            isTemperatureSensitive,
            shelfLifeDays,
            photoFileName,
            description);
    }

    /// <summary>
    /// Updates the editable product data.
    /// </summary>
    /// <param name="manufacturer">Manufacturer name.</param>
    /// <param name="name">Product name.</param>
    /// <param name="category">Catalogue category.</param>
    /// <param name="weightGrams">Weight of a single unit, in grams.</param>
    /// <param name="price">Price of a single unit, in rubles.</param>
    /// <param name="isTemperatureSensitive">Temperature sensitive flag.</param>
    /// <param name="shelfLifeDays">Shelf life, in days.</param>
    /// <param name="photoFileName">Photo file name or <see langword="null"/>.</param>
    /// <param name="description">Product description or <see langword="null"/>.</param>
    /// <exception cref="DomainException">Product data is not valid.</exception>
    public void Update(
        string manufacturer,
        string name,
        string category,
        int weightGrams,
        decimal price,
        bool isTemperatureSensitive,
        int shelfLifeDays,
        string? photoFileName,
        string? description)
    {
        Validate(Id, manufacturer, name, category, weightGrams, price, shelfLifeDays);

        Manufacturer = manufacturer.Trim();
        Name = name.Trim();
        Category = category.Trim();
        WeightGrams = weightGrams;
        Price = price;
        IsTemperatureSensitive = isTemperatureSensitive;
        ShelfLifeDays = shelfLifeDays;
        PhotoFileName = photoFileName;
        Description = description;
    }

    /// <summary>
    /// Hides the product from the catalogue (soft deletion).
    /// </summary>
    public void Hide() => IsActive = false;

    /// <summary>
    /// Returns the product to the catalogue.
    /// </summary>
    public void Show() => IsActive = true;

    private static void Validate(
        string id,
        string manufacturer,
        string name,
        string category,
        int weightGrams,
        decimal price,
        int shelfLifeDays)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new DomainException("У товара должен быть идентификатор.");
        }

        if (string.IsNullOrWhiteSpace(manufacturer))
        {
            throw new DomainException("У товара должен быть производитель.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("У товара должно быть наименование.");
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new DomainException("У товара должна быть категория.");
        }

        if (weightGrams <= 0)
        {
            throw new DomainException("Вес товара должен быть больше нуля.");
        }

        if (price < 0m)
        {
            throw new DomainException("Цена товара не может быть отрицательной.");
        }

        if (shelfLifeDays <= 0)
        {
            throw new DomainException("Срок годности товара должен быть больше нуля.");
        }
    }
}