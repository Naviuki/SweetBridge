using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Кондитерское изделие одной из витрин.
/// </summary>
public sealed class Product
{
    private Product()
    {
    }

    private Product(
        string id,
        int producerId,
        Region region,
        string name,
        string category,
        int weightGrams,
        decimal price,
        bool isTemperatureSensitive,
        int shelfLifeDays,
        string? photoPath,
        string? description)
    {
        Id = id;
        ProducerId = producerId;
        Region = region;
        Name = name;
        Category = category;
        WeightGrams = weightGrams;
        Price = price;
        IsTemperatureSensitive = isTemperatureSensitive;
        ShelfLifeDays = shelfLifeDays;
        PhotoPath = photoPath;
        Description = description;
        IsActive = true;
    }

    /// <summary>
    /// Идентификатор товара в формате <c>YKT-001</c>.
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Идентификатор производителя.
    /// </summary>
    public int ProducerId { get; private set; }

    /// <summary>
    /// Регион производства, берётся у производителя.
    /// </summary>
    public Region Region { get; private set; }

    /// <summary>
    /// Название производителя.
    /// </summary>
    public Producer? Producer { get; private set; }

    /// <summary>
    /// Наименование товара.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Категория каталога.
    /// </summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// Вес одной штуки в граммах.
    /// </summary>
    public int WeightGrams { get; private set; }

    /// <summary>
    /// Цена одной штуки в рублях.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Признак термочувствительности.
    /// </summary>
    public bool IsTemperatureSensitive { get; private set; }

    /// <summary>
    /// Срок годности в днях.
    /// </summary>
    public int ShelfLifeDays { get; private set; }

    /// <summary>
    /// Путь к фотографии внутри <c>wwwroot/uploads/products</c>.
    /// </summary>
    public string? PhotoPath { get; private set; }

    /// <summary>
    /// Описание товара.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Признак мягкого удаления.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Направление доставки: товары региона едут в противоположный регион.
    /// </summary>
    public ShippingDirection ShippingDirection => Region == Region.Yakutia
        ? ShippingDirection.YakutiaToDonbass
        : ShippingDirection.DonbassToYakutia;

    /// <summary>
    /// Создаёт товар.
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <param name="producerId">Производитель.</param>
    /// <param name="region">Регион производства.</param>
    /// <param name="name">Наименование.</param>
    /// <param name="category">Категория.</param>
    /// <param name="weightGrams">Вес в граммах.</param>
    /// <param name="price">Цена в рублях.</param>
    /// <param name="isTemperatureSensitive">Термочувствительность.</param>
    /// <param name="shelfLifeDays">Срок годности в днях.</param>
    /// <param name="photoPath">Путь к фотографии.</param>
    /// <param name="description">Описание.</param>
    /// <returns>Новый товар.</returns>
    /// <exception cref="DomainException">Данные товара некорректны.</exception>
    public static Product Create(
        string id,
        int producerId,
        Region region,
        string name,
        string category,
        int weightGrams,
        decimal price,
        bool isTemperatureSensitive,
        int shelfLifeDays,
        string? photoPath,
        string? description)
    {
        Validate(id, name, category, weightGrams, price, shelfLifeDays);

        return new Product(
            id.Trim().ToUpperInvariant(),
            producerId,
            region,
            name.Trim(),
            category.Trim(),
            weightGrams,
            price,
            isTemperatureSensitive,
            shelfLifeDays,
            photoPath,
            description);
    }

    /// <summary>
    /// Обновляет редактируемые данные товара.
    /// </summary>
    /// <param name="producerId">Производитель.</param>
    /// <param name="region">Регион производства.</param>
    /// <param name="name">Наименование.</param>
    /// <param name="category">Категория.</param>
    /// <param name="weightGrams">Вес в граммах.</param>
    /// <param name="price">Цена в рублях.</param>
    /// <param name="isTemperatureSensitive">Термочувствительность.</param>
    /// <param name="shelfLifeDays">Срок годности.</param>
    /// <param name="photoPath">Путь к фотографии.</param>
    /// <param name="description">Описание.</param>
    /// <exception cref="DomainException">Данные товара некорректны.</exception>
    public void Update(
        int producerId,
        Region region,
        string name,
        string category,
        int weightGrams,
        decimal price,
        bool isTemperatureSensitive,
        int shelfLifeDays,
        string? photoPath,
        string? description)
    {
        Validate(Id, name, category, weightGrams, price, shelfLifeDays);

        ProducerId = producerId;
        Region = region;
        Name = name.Trim();
        Category = category.Trim();
        WeightGrams = weightGrams;
        Price = price;
        IsTemperatureSensitive = isTemperatureSensitive;
        ShelfLifeDays = shelfLifeDays;
        PhotoPath = photoPath;
        Description = description;
    }

    /// <summary>
    /// Скрывает товар из каталога.
    /// </summary>
    public void Hide() => IsActive = false;

    /// <summary>
    /// Возвращает товар в каталог.
    /// </summary>
    public void Show() => IsActive = true;

    private static void Validate(
        string id,
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