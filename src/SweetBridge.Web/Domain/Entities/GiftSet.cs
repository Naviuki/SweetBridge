using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Готовый подарочный набор: фиксированный состав, цена и оформление.
/// </summary>
public sealed class GiftSet
{
    private readonly List<GiftSetItem> _items = [];

    private GiftSet()
    {
    }

    private GiftSet(string name, Region showcase, string description, string? photoPath, decimal price)
    {
        Name = name;
        Showcase = showcase;
        Description = description;
        PhotoPath = photoPath;
        Price = price;
        IsActive = true;
    }

    /// <summary>
    /// Идентификатор набора.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Название набора.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Витрина, из которой собран набор.
    /// </summary>
    public Region Showcase { get; private set; }

    /// <summary>
    /// Описание набора.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Путь к фотографии.
    /// </summary>
    public string? PhotoPath { get; private set; }

    /// <summary>
    /// Цена набора в рублях.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Признак активности.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Состав набора.
    /// </summary>
    public IReadOnlyList<GiftSetItem> Items => _items;

    /// <summary>
    /// Суммарный вес набора в граммах.
    /// </summary>
    public int TotalWeightGrams => _items.Sum(item => item.LineWeightGrams);

    /// <summary>
    /// Создаёт набор.
    /// </summary>
    /// <param name="name">Название.</param>
    /// <param name="showcase">Витрина.</param>
    /// <param name="description">Описание.</param>
    /// <param name="photoPath">Путь к фотографии.</param>
    /// <param name="price">Цена.</param>
    /// <returns>Новый набор.</returns>
    /// <exception cref="DomainException">Данные набора некорректны.</exception>
    public static GiftSet Create(string name, Region showcase, string description, string? photoPath, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Укажите название подарочного набора.");
        }

        if (price < 0m)
        {
            throw new DomainException("Цена подарочного набора не может быть отрицательной.");
        }

        return new GiftSet(name.Trim(), showcase, description.Trim(), photoPath, price);
    }

    /// <summary>
    /// Добавляет товар в состав набора.
    /// </summary>
    /// <param name="item">Позиция набора.</param>
    /// <exception cref="DomainException">Позиция уже добавлена.</exception>
    public void AddItem(GiftSetItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_items.Any(existing => string.Equals(existing.ProductId, item.ProductId, StringComparison.Ordinal)))
        {
            throw new DomainException("Товар уже включён в состав набора.");
        }

        _items.Add(item);
    }

    /// <summary>
    /// Обновляет данные набора.
    /// </summary>
    /// <param name="name">Название.</param>
    /// <param name="showcase">Витрина.</param>
    /// <param name="description">Описание.</param>
    /// <param name="photoPath">Путь к фотографии.</param>
    /// <param name="price">Цена.</param>
    /// <exception cref="DomainException">Данные набора некорректны.</exception>
    public void Update(string name, Region showcase, string description, string? photoPath, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Укажите название подарочного набора.");
        }

        if (price < 0m)
        {
            throw new DomainException("Цена подарочного набора не может быть отрицательной.");
        }

        Name = name.Trim();
        Showcase = showcase;
        Description = description.Trim();
        PhotoPath = photoPath;
        Price = price;
    }

    /// <summary>
    /// Скрывает набор.
    /// </summary>
    public void Hide() => IsActive = false;

    /// <summary>
    /// Возвращает набор в витрину.
    /// </summary>
    public void Show() => IsActive = true;
}