using CsvHelper.Configuration.Attributes;

namespace SweetWave.Web.Infrastructure.Seeding;

/// <summary>
/// Одна строка CSV-файла каталога.
/// </summary>
public sealed class CatalogCsvRecord
{
    /// <summary>
    /// Идентификатор товара.
    /// </summary>
    [Name("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Регион производителя: <c>Якутия</c> или <c>ДНР</c>.
    /// </summary>
    [Name("region")]
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Название производителя.
    /// </summary>
    [Name("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// Наименование товара.
    /// </summary>
    [Name("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Категория в каталоге.
    /// </summary>
    [Name("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Вес одной штуки в граммах.
    /// </summary>
    [Name("weight_g")]
    public int WeightGrams { get; set; }

    /// <summary>
    /// Цена одной штуки в рублях.
    /// </summary>
    [Name("price_rub")]
    public decimal Price { get; set; }

    /// <summary>
    /// Признак температурной чувствительности: <c>да</c> или <c>нет</c>.
    /// </summary>
    [Name("temp_sensitive")]
    public string TempSensitive { get; set; } = string.Empty;

    /// <summary>
    /// Срок годности в днях.
    /// </summary>
    [Name("shelf_life_days")]
    public int ShelfLifeDays { get; set; }

    /// <summary>
    /// Имя файла изображения в каталоге <c>wwwroot/images</c>.
    /// </summary>
    [Name("photo")]
    public string? Photo { get; set; }

    /// <summary>
    /// Описание товара.
    /// </summary>
    [Name("description")]
    public string? Description { get; set; }
}
