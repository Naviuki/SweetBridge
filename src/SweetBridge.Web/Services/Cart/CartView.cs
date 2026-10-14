using System.Globalization;

namespace SweetBridge.Web.Services.Cart;

/// <summary>
/// Модель чтения корзины.
/// </summary>
/// <param name="Lines">Позиции корзины.</param>
public sealed record CartView(IReadOnlyList<CartLineView> Lines)
{
    /// <summary>
    /// Суммарное количество товаров.
    /// </summary>
    public int ItemCount => Lines.Sum(line => line.Quantity);

    /// <summary>
    /// Суммарный вес корзины в граммах.
    /// </summary>
    public int TotalWeightGrams => Lines.Sum(line => line.LineWeightGrams);

    /// <summary>
    /// Стоимость товаров в рублях.
    /// </summary>
    public decimal GoodsCost => Lines.Sum(line => line.LineTotal);

    /// <summary>
    /// Признак того, что корзина пуста.
    /// </summary>
    public bool IsEmpty => Lines.Count == 0;

    /// <summary>
    /// Пустая корзина.
    /// </summary>
    /// <returns>Пустая корзина.</returns>
    public static CartView Empty() => new([]);

    /// <summary>
    /// Направление доставки корзины, если она не пуста.
    /// </summary>
    /// <returns>Направление доставки или <see langword="null"/>.</returns>
    public Domain.Enums.ShippingDirection? ShippingDirection =>
        Lines.Select(line => line.ShippingDirection).Distinct().Count() == 1
            ? Lines[0].ShippingDirection
            : null;
}