using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Позиция состава подарочного набора.
/// </summary>
public sealed class GiftSetItem
{
    private GiftSetItem()
    {
    }

    private GiftSetItem(int giftSetId, string productId, string productName, int quantity, int weightGrams)
    {
        GiftSetId = giftSetId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        WeightGrams = weightGrams;
    }

    /// <summary>
    /// Идентификатор позиции.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Набор, в состав которого входит позиция.
    /// </summary>
    public int GiftSetId { get; private set; }

    /// <summary>
    /// Идентификатор товара.
    /// </summary>
    public string ProductId { get; private set; } = string.Empty;

    /// <summary>
    /// Наименование товара на момент сохранения.
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>
    /// Количество штук.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Вес одной штуки в граммах.
    /// </summary>
    public int WeightGrams { get; private set; }

    /// <summary>
    /// Суммарный вес позиции в граммах.
    /// </summary>
    public int LineWeightGrams => Quantity * WeightGrams;

    /// <summary>
    /// Создаёт позицию набора.
    /// </summary>
    /// <param name="giftSetId">Набор.</param>
    /// <param name="productId">Товар.</param>
    /// <param name="productName">Наименование товара.</param>
    /// <param name="quantity">Количество.</param>
    /// <param name="weightGrams">Вес одной штуки.</param>
    /// <returns>Новая позиция.</returns>
    /// <exception cref="DomainException">Данные позиции некорректны.</exception>
    public static GiftSetItem Create(int giftSetId, string productId, string productName, int quantity, int weightGrams)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new DomainException("Укажите товар в составе набора.");
        }

        if (quantity <= 0 || weightGrams <= 0)
        {
            throw new DomainException("Количество и вес в составе набора должны быть больше нуля.");
        }

        return new GiftSetItem(giftSetId, productId.Trim().ToUpperInvariant(), productName.Trim(), quantity, weightGrams);
    }
}