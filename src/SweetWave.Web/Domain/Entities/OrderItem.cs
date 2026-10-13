using SweetWave.Web.Domain.Exceptions;

namespace SweetWave.Web.Domain.Entities;

/// <summary>
/// Одна позиция заказа. Данные товара фиксируются на момент оформления.
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
    /// Идентификатор позиции.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Идентификатор заказа, которому принадлежит позиция.
    /// </summary>
    public int OrderId { get; private set; }

    /// <summary>
    /// Идентификатор товара в каталоге.
    /// </summary>
    public string ProductId { get; private set; } = string.Empty;

    /// <summary>
    /// Наименование товара на момент заказа.
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>
    /// Цена одной штуки на момент заказа в рублях.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Вес одной штуки на момент заказа в граммах.
    /// </summary>
    public int WeightGrams { get; private set; }

    /// <summary>
    /// Заказанное количество.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Общий вес позиции в граммах.
    /// </summary>
    public int LineWeightGrams => WeightGrams * Quantity;

    /// <summary>
    /// Итоговая стоимость позиции в рублях.
    /// </summary>
    public decimal LineTotal => UnitPrice * Quantity;

    /// <summary>
    /// Создаёт позицию заказа.
    /// </summary>
    /// <param name="productId">Идентификатор товара в каталоге.</param>
    /// <param name="productName">Наименование товара.</param>
    /// <param name="unitPrice">Цена одной штуки в рублях.</param>
    /// <param name="weightGrams">Вес одной штуки в граммах.</param>
    /// <param name="quantity">Заказанное количество.</param>
    /// <returns>Созданная позиция.</returns>
    /// <exception cref="DomainException">Данные позиции заказа некорректны.</exception>
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
