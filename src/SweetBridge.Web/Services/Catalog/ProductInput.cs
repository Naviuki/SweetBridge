using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Catalog;

/// <summary>
/// Данные товара, создаваемого или редактируемого в админке.
/// </summary>
/// <param name="Id">Идентификатор товара при создании, игнорируется при правке.</param>
/// <param name="ProducerName">Название производителя: если такого нет, он создаётся.</param>
/// <param name="Region">Регион производителя.</param>
/// <param name="Name">Наименование товара.</param>
/// <param name="Category">Категория в каталоге.</param>
/// <param name="WeightGrams">Вес одной штуки в граммах.</param>
/// <param name="Price">Цена одной штуки в рублях.</param>
/// <param name="IsTemperatureSensitive">Признак температурной чувствительности.</param>
/// <param name="ShelfLifeDays">Срок годности в днях.</param>
/// <param name="PhotoPath">Путь к фотографии внутри uploads или <see langword="null"/>.</param>
/// <param name="Description">Описание товара или <see langword="null"/>.</param>
public sealed record ProductInput(
    string Id,
    string ProducerName,
    Region Region,
    string Name,
    string Category,
    int WeightGrams,
    decimal Price,
    bool IsTemperatureSensitive,
    int ShelfLifeDays,
    string? PhotoPath,
    string? Description);