using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;

namespace SweetWave.Web.Domain.Entities;

/// <summary>
/// Кондитерское изделие одной из двух витрин.
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
        string? photo,
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
        Photo = photo;
        Description = description;
        IsActive = true;
    }

    /// <summary>
    /// Идентификатор товара в формате каталога, например <c>YKT-001</c>.
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Регион производителя.
    /// </summary>
    public Region Region { get; private set; }

    /// <summary>
    /// Название производителя.
    /// </summary>
    public string Manufacturer { get; private set; } = string.Empty;

    /// <summary>
    /// Наименование товара.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Категория каталога, используемая в фильтре.
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
    /// Признак того, что товар нельзя отправлять без термоупаковки в холодные и жаркие месяцы.
    /// </summary>
    public bool IsTemperatureSensitive { get; private set; }

    /// <summary>
    /// Срок годности в днях.
    /// </summary>
    public int ShelfLifeDays { get; private set; }

    /// <summary>
    /// Имя файла изображения в каталоге <c>wwwroot/images</c> или <see langword="null"/> для заглушки.
    /// </summary>
    public string? Photo { get; private set; }

    /// <summary>
    /// Описание товара, показываемое на карточке.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Признак мягкого удаления: скрытые товары остаются в прошлых заказах, но исчезают из каталога.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Направление доставки: товары региона всегда едут в противоположный регион.
    /// </summary>
    public ShippingDirection ShippingDirection => Region == Region.Yakutia
        ? ShippingDirection.YakutiaToDonbass
        : ShippingDirection.DonbassToYakutia;

    /// <summary>
    /// Создаёт новый активный товар.
    /// </summary>
    /// <param name="id">Уникальный идентификатор товара.</param>
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
    /// <returns>Созданный товар.</returns>
    /// <exception cref="DomainException">Данные товара некорректны.</exception>
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
        string? photo,
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
            photo,
            description);
    }

    /// <summary>
    /// Обновляет изменяемые данные товара.
    /// </summary>
    /// <param name="manufacturer">Название производителя.</param>
    /// <param name="name">Наименование товара.</param>
    /// <param name="category">Категория в каталоге.</param>
    /// <param name="weightGrams">Вес одной штуки в граммах.</param>
    /// <param name="price">Цена одной штуки в рублях.</param>
    /// <param name="isTemperatureSensitive">Признак температурной чувствительности.</param>
    /// <param name="shelfLifeDays">Срок годности в днях.</param>
    /// <param name="photo">Имя файла изображения или <see langword="null"/>.</param>
    /// <param name="description">Описание товара или <see langword="null"/>.</param>
    /// <exception cref="DomainException">Данные товара некорректны.</exception>
    public void Update(
        string manufacturer,
        string name,
        string category,
        int weightGrams,
        decimal price,
        bool isTemperatureSensitive,
        int shelfLifeDays,
        string? photo,
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
        Photo = photo;
        Description = description;
    }

    /// <summary>
    /// Скрывает товар из каталога (мягкое удаление).
    /// </summary>
    public void Hide() => IsActive = false;

    /// <summary>
    /// Возвращает товар в каталог.
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
