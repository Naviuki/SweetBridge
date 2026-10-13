using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Модель чтения позиции заказа.
/// </summary>
/// <param name="ProductName">Наименование товара на момент заказа.</param>
/// <param name="unitPrice">Цена одной штуки в рублях.</param>
/// <param name="quantity">Заказанное количество.</param>
public sealed record OrderLineSummary(string ProductName, decimal UnitPrice, int Quantity)
{
    /// <summary>
    /// Итоговая стоимость позиции в рублях.
    /// </summary>
    public decimal LineTotal => UnitPrice * Quantity;
}
