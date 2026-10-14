using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Производитель кондитерских изделий.
/// </summary>
public sealed class Producer
{
    private Producer()
    {
    }

    private Producer(string name, Region region, string? description)
    {
        Name = name;
        Region = region;
        Description = description;
        IsActive = true;
    }

    /// <summary>
    /// Идентификатор производителя.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Название производителя.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Регион производства.
    /// </summary>
    public Region Region { get; private set; }

    /// <summary>
    /// Описание производителя.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Признак активности: скрытые производители не показываются в каталоге.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Создаёт производителя.
    /// </summary>
    /// <param name="name">Название.</param>
    /// <param name="region">Регион производства.</param>
    /// <param name="description">Описание.</param>
    /// <returns>Новый производитель.</returns>
    /// <exception cref="DomainException">Название пустое.</exception>
    public static Producer Create(string name, Region region, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Укажите название производителя.");
        }

        return new Producer(name.Trim(), region, description?.Trim());
    }

    /// <summary>
    /// Обновляет данные производителя.
    /// </summary>
    /// <param name="name">Название.</param>
    /// <param name="region">Регион производства.</param>
    /// <param name="description">Описание.</param>
    /// <exception cref="DomainException">Название пустое.</exception>
    public void Update(string name, Region region, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Укажите название производителя.");
        }

        Name = name.Trim();
        Region = region;
        Description = description?.Trim();
    }

    /// <summary>
    /// Скрывает производителя.
    /// </summary>
    public void Hide() => IsActive = false;

    /// <summary>
    /// Возвращает производителя в каталог.
    /// </summary>
    public void Show() => IsActive = true;
}