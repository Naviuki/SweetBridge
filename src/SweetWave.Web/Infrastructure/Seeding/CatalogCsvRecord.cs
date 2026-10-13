using CsvHelper.Configuration.Attributes;

namespace SweetWave.Web.Infrastructure.Seeding;

/// <summary>
/// One row of the catalogue CSV file.
/// </summary>
public sealed class CatalogCsvRecord
{
    /// <summary>
    /// Product identifier.
    /// </summary>
    [Name("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Region of the manufacturer: <c>Якутия</c> or <c>ДНР</c>.
    /// </summary>
    [Name("region")]
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Manufacturer name.
    /// </summary>
    [Name("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// Product name.
    /// </summary>
    [Name("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Catalogue category.
    /// </summary>
    [Name("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Weight of a single unit, in grams.
    /// </summary>
    [Name("weight_g")]
    public int WeightGrams { get; set; }

    /// <summary>
    /// Price of a single unit, in rubles.
    /// </summary>
    [Name("price_rub")]
    public decimal Price { get; set; }

    /// <summary>
    /// Temperature sensitive flag: <c>да</c> or <c>нет</c>.
    /// </summary>
    [Name("temp_sensitive")]
    public string TempSensitive { get; set; } = string.Empty;

    /// <summary>
    /// Shelf life, in days.
    /// </summary>
    [Name("shelf_life_days")]
    public int ShelfLifeDays { get; set; }

    /// <summary>
    /// Photo file name inside <c>wwwroot/images</c>.
    /// </summary>
    [Name("photo")]
    public string? Photo { get; set; }

    /// <summary>
    /// Product description.
    /// </summary>
    [Name("description")]
    public string? Description { get; set; }
}