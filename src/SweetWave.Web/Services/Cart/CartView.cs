using SweetWave.Web.Services.Catalog;

namespace SweetWave.Web.Services.Cart;

/// <summary>
/// Модель чтения корзины.
/// </summary>
/// <param name="Lines">Позиции корзины.</param>
public sealed record CartView(IReadOnlyList<CartLineView> Lines)
{
    /// <summary>
    /// Признак того, что в корзине нет позиций.
    /// </summary>
    public bool IsEmpty => Lines.Count == 0;

    /// <summary>
    /// Направление витрины, к которой относится корзина, или <see langword="null"/> для пустой корзины.
    /// </summary>
    public Domain.Enums.ShippingDirection? Direction => Lines.Count == 0 ? null : Lines[0].Product.ShippingDirection;

    /// <summary>
    /// Общее количество товаров в корзине.
    /// </summary>
    public int TotalQuantity => Lines.Sum(line => line.Quantity);

    /// <summary>
    /// Общий вес корзины в граммах.
    /// </summary>
    public int TotalWeightGrams => Lines.Sum(line => line.LineWeightGrams);

    /// <summary>
    /// Итоговая стоимость товаров в рублях.
    /// </summary>
    public decimal GoodsCost => Lines.Sum(line => line.LineTotal);

    /// <summary>
    /// Признак того, что в корзине есть температурно чувствительные товары.
    /// </summary>
    public bool HasTemperatureSensitiveItems => Lines.Any(line => line.Product.IsTemperatureSensitive);

    /// <summary>
    /// Товары каталога, находящиеся в корзине.
    /// </summary>
    public IReadOnlyList<ProductSummary> Products => [.. Lines.Select(line => line.Product)];
}
