using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Read model of an order position.
/// </summary>
/// <param name="ProductName">Product name at the moment of ordering.</param>
/// <param name="UnitPrice">Price of a single unit, in rubles.</param>
/// <param name="Quantity">Ordered quantity.</param>
public sealed record OrderLineSummary(string ProductName, decimal UnitPrice, int Quantity)
{
    /// <summary>
    /// Total cost of the position, in rubles.
    /// </summary>
    public decimal LineTotal => UnitPrice * Quantity;
}