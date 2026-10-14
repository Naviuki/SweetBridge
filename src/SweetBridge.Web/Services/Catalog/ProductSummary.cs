using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Catalog;

/// <summary>
/// Модель чтения товара каталога.
/// </summary>
/// <param name="Id">Идентификатор товара.</param>
/// <param name="Region">Регион производителя.</param>
/// <param name="ProducerName">Название производителя.</param>
/// <param name="Name">Наименование товара.</param>
/// <param name="Category">Категория в каталоге.</param>
/// <param name="WeightGrams">Вес одной штуки в граммах.</param>
/// <param name="Price">Цена одной штуки в рублях.</param>
/// <param name="IsTemperatureSensitive">Признак температурной чувствительности.</param>
/// <param name="ShelfLifeDays">Срок годности в днях.</param>
/// <param name="PhotoPath">Путь к фотографии или <see langword="null"/>.</param>
/// <param name="Description">Описание товара или <see langword="null"/>.</param>
/// <param name="ShippingDirection">Направление доставки товара.</param>
/// <param name="IsActive">Признак того, что товар виден в каталоге.</param>
public sealed record ProductSummary(
    string Id,
    Region Region,
    string ProducerName,
    string Name,
    string Category,
    int WeightGrams,
    decimal Price,
    bool IsTemperatureSensitive,
    int ShelfLifeDays,
    string? PhotoPath,
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
            product.Producer?.Name ?? string.Empty,
            product.Name,
            product.Category,
            product.WeightGrams,
            product.Price,
            product.IsTemperatureSensitive,
            product.ShelfLifeDays,
            product.PhotoPath,
            product.Description,
            product.ShippingDirection,
            product.IsActive);
    }
}