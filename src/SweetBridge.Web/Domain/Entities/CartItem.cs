using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Позиция корзины, сохранённая в базе для авторизованного пользователя.
/// Гостявая корзина живёт в сессии, при входе она сливается с этой.
/// </summary>
public sealed class CartItem
{
    private CartItem()
    {
    }

    private CartItem(string userId, string productId, int quantity, DateTime addedAtUtc)
    {
        UserId = userId;
        ProductId = productId;
        Quantity = quantity;
        AddedAtUtc = addedAtUtc;
    }

    /// <summary>
    /// Идентификатор позиции.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Идентификатор товара.
    /// </summary>
    public string ProductId { get; private set; } = string.Empty;

    /// <summary>
    /// Количество штук.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Момент добавления, в UTC.
    /// </summary>
    public DateTime AddedAtUtc { get; private set; }

    /// <summary>
    /// Добавляет товар в корзину пользователя.
    /// </summary>
    /// <param name="userId">Пользователь.</param>
    /// <param name="productId">Товар.</param>
    /// <param name="quantity">Количество.</param>
    /// <param name="addedAtUtc">Момент добавления.</param>
    /// <returns>Новая позиция корзины.</returns>
    /// <exception cref="DomainException">Количество некорректно.</exception>
    public static CartItem Create(string userId, string productId, int quantity, DateTime addedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(productId))
        {
            throw new DomainException("Нельзя добавить товар в корзину без пользователя и товара.");
        }

        return new CartItem(userId, productId, Normalize(quantity), addedAtUtc);
    }

    /// <summary>
    /// Устанавливает новое количество с ограничением диапазона.
    /// </summary>
    /// <param name="quantity">Новое количество.</param>
    public void SetQuantity(int quantity) => Quantity = Normalize(quantity);

    /// <summary>
    /// Прибавляет количество к позиции.
    /// </summary>
    /// <param name="delta">Приращение.</param>
    /// <exception cref="DomainException">Количество выходит за границы.</exception>
    public void Increase(int delta)
    {
        int next = Quantity + delta;

        if (next > CartLimits.MaxQuantityPerProduct)
        {
            throw new DomainException($"Можно добавить не больше {CartLimits.MaxQuantityPerProduct} штук одного товара.");
        }

        Quantity = next;
    }

    /// <summary>
    /// Прибавляет количество без исключения: значение обрезается по границам.
    /// </summary>
    /// <param name="delta">Приращение.</param>
    public void IncreaseClamped(int delta) => Quantity = Math.Clamp(Quantity + delta, 1, CartLimits.MaxQuantityPerProduct);

    private static int Normalize(int quantity)
    {
        if (quantity is < 1 or > CartLimits.MaxQuantityPerProduct)
        {
            throw new DomainException($"Количество должно быть от 1 до {CartLimits.MaxQuantityPerProduct} штук.");
        }

        return quantity;
    }
}