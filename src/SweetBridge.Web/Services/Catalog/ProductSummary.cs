using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Catalog;

/// <summary>
/// Модель чтения товара каталога.
/// </summary>
/// <param name="Id">Идентификатор товара.</param>
/// <param name="region">Регион производителя.</param>
/// <param name="manufacturer">Название производителя.</param>
/// <param name="name">Наименование товара.</param>
/// <param name="category">Категория в каталоге.</param>
/// <param name="weightGrams">Вес одной штуки в граммах.</param>
/// <param name="price">Цена одной штуки в рублях.</param>
/// <param name="isTemperatureSensitive">Признак температурной чувствительности.</param>
/// <param name="shelfLifeDays">Срок годности в днях.</param>
/// <param name="photo">Имя файла изображения или <see langword="null"/>.</param>
/// <param name="description">Описание товара или <see langword="null"/>.</param>
/// <param name="ShippingDirection">Направление доставки товара.</param>
/// <param name="IsActive">Признак того, что товар виден в каталоге.</param>
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
    ShippingDirection ShippingDirection,
    bool IsActive)
{
    /// <summary>
    /// Создаёт модель чтения из товара предметной области.
    /// </summary>
    /// <param name="product">Товар предметной области.</param>
    /// <returns>Модель чтения товара.</returns>
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
            product.ShippingDirection,
            product.IsActive);
    }
}
