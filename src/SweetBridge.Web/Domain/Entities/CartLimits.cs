namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Границы корзины, общие для домена и сессионной корзины.
/// </summary>
public static class CartLimits
{
    /// <summary>
    /// Максимальное количество штук одного товара в корзине.
    /// </summary>
    public const int MaxQuantityPerProduct = 99;

    /// <summary>
    /// Максимальное количество позиций в корзине.
    /// </summary>
    public const int MaxLines = 30;
}