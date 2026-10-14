using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Catalog;

/// <summary>
/// Данные товара, создаваемого или редактируемого в админке.
/// </summary>
/// <param name="Id">Идентификатор товара при создании, игнорируется при правке.</param>
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
