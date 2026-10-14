using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Пункт выдачи заказов в регионе назначения.
/// </summary>
public sealed class PickupPoint
{
    private PickupPoint()
    {
    }

    private PickupPoint(Enums.Region region, string city, string number, string address)
    {
        Region = region;
        City = city;
        Number = number;
        Address = address;
        IsActive = true;
    }

    /// <summary>
    /// Идентификатор пункта выдачи.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Регион, в котором находится пункт.
    /// </summary>
    public Enums.Region Region { get; private set; }

    /// <summary>
    /// Город.
    /// </summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>
    /// Номер пункта выдачи.
    /// </summary>
    public string Number { get; private set; } = string.Empty;

    /// <summary>
    /// Адрес пункта выдачи.
    /// </summary>
    public string Address { get; private set; } = string.Empty;

    /// <summary>
    /// Признак активности.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Создаёт пункт выдачи.
    /// </summary>
    /// <param name="region">Регион.</param>
    /// <param name="city">Город.</param>
    /// <param name="number">Номер пункта.</param>
    /// <param name="address">Адрес.</param>
    /// <returns>Новый пункт выдачи.</returns>
    /// <exception cref="Exceptions.DomainException">Обязательные поля не заполнены.</exception>
    public static PickupPoint Create(Enums.Region region, string city, string number, string address)
    {
        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(number) || string.IsNullOrWhiteSpace(address))
        {
            throw new DomainException("Заполните город, номер и адрес пункта выдачи.");
        }

        return new PickupPoint(region, city.Trim(), number.Trim(), address.Trim());
    }

    /// <summary>
    /// Обновляет данные пункта выдачи.
    /// </summary>
    /// <param name="region">Регион.</param>
    /// <param name="city">Город.</param>
    /// <param name="number">Номер пункта.</param>
    /// <param name="address">Адрес.</param>
    /// <exception cref="DomainException">Обязательные поля не заполнены.</exception>
    public void Update(Enums.Region region, string city, string number, string address)
    {
        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(number) || string.IsNullOrWhiteSpace(address))
        {
            throw new DomainException("Заполните город, номер и адрес пункта выдачи.");
        }

        Region = region;
        City = city.Trim();
        Number = number.Trim();
        Address = address.Trim();
    }

    /// <summary>
    /// Скрывает пункт выдачи.
    /// </summary>
    public void Hide() => IsActive = false;

    /// <summary>
    /// Возвращает пункт выдачи в список.
    /// </summary>
    public void Show() => IsActive = true;
}