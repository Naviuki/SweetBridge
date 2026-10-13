using SweetWave.Web.Domain.Exceptions;

namespace SweetWave.Web.Domain.Entities;

/// <summary>
/// A single position of an order. Product data is captured at the moment of ordering.
/// </summary>
public sealed class OrderItem
{
    private OrderItem()
    {
    }

    private OrderItem(string productId, string productName, decimal unitPrice, int weightGrams, int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        WeightGrams = weightGrams;
        Quantity = quantity;
    }

    /// <summary>
    /// Position identifier.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Identifier of the order the position belongs to.
    /// </summary>
    public int OrderId { get; private set; }

    /// <summary>
    /// Catalogue identifier of the product.
    /// </summary>
    public string ProductId { get; private set; } = string.Empty;

    /// <summary>
    /// Product name at the moment of ordering.
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>
    /// Price of a single unit at the moment of ordering, in rubles.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Weight of a single unit at the moment of ordering, in grams.
    /// </summary>
    public int WeightGrams { get; private set; }

    /// <summary>
    /// Ordered quantity.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Total weight of the position, in grams.
    /// </summary>
    public int LineWeightGrams => WeightGrams * Quantity;

    /// <summary>
    /// Total cost of the position, in rubles.
    /// </summary>
    public decimal LineTotal => UnitPrice * Quantity;

    /// <summary>
    /// Creates an order position.
    /// </summary>
    /// <param name="productId">Catalogue identifier of the product.</param>
    /// <param name="productName">Product name.</param>
    /// <param name="unitPrice">Price of a single unit, in rubles.</param>
    /// <param name="weightGrams">Weight of a single unit, in grams.</param>
    /// <param name="quantity">Ordered quantity.</param>
    /// <returns>Created position.</returns>
    /// <exception cref="DomainException">Position data is not valid.</exception>
    public static OrderItem Create(string productId, string productName, decimal unitPrice, int weightGrams, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new DomainException("Позиция заказа должна ссылаться на товар каталога.");
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new DomainException("Позиция заказа должна содержать наименование товара.");
        }

        if (unitPrice < 0m)
        {
            throw new DomainException("Цена позиции заказа не может быть отрицательной.");
        }

        if (weightGrams <= 0)
        {
            throw new DomainException("Вес позиции заказа должен быть больше нуля.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Количество в позиции заказа должно быть больше нуля.");
        }

        return new OrderItem(productId, productName.Trim(), unitPrice, weightGrams, quantity);
    }
}