using SweetWave.Web.Services.Catalog;

namespace SweetWave.Web.Services.Cart;

/// <summary>
/// Модель чтения позиции корзины: товар каталога и заказанное количество.
/// </summary>
/// <param name="Product">Товар каталога с актуальными ценой и весом.</param>
/// <param name="quantity">Заказанное количество.</param>
public sealed record CartLineView(ProductSummary Product, int Quantity)
{
    /// <summary>
    /// Общий вес позиции в граммах.
    /// </summary>
    public int LineWeightGrams => Product.WeightGrams * Quantity;

    /// <summary>
    /// Итоговая стоимость позиции в рублях.
    /// </summary>
    public decimal LineTotal => Product.Price * Quantity;
}
