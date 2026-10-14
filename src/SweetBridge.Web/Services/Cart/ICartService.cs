namespace SweetBridge.Web.Services.Cart;

/// <summary>
/// Корзина покупок, хранящаяся в сессии.
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Возвращает количество товаров в корзине без обращения к базе данных.
    /// </summary>
    /// <returns>Общее количество товаров в корзине.</returns>
    int GetItemCount();

    /// <summary>
    /// Возвращает количество товара в корзине без обращения к базе данных.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <returns>Количество товара в корзине.</returns>
    int GetQuantity(string productId);

    /// <summary>
    /// Добавляет товар в корзину без ожидания: используется страницами каталога,
    /// где обработчик формы уже выполняет запись в корзину перед редиректом.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="quantity">Количество для добавления.</param>
    void Add(string productId, int quantity);

    /// <summary>
    /// Возвращает корзину с актуальными ценами и весами.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Текущая корзина.</returns>
    Task<CartView> GetCartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Добавляет товар в корзину.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="quantity">Количество для добавления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Обновлённая корзина.</returns>
    Task<CartView> AddItemAsync(string productId, int quantity, CancellationToken cancellationToken);

    /// <summary>
    /// Задаёт количество в позиции: количество меньше одного удаляет позицию.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="quantity">Новое количество.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Обновлённая корзина.</returns>
    Task<CartView> SetQuantityAsync(string productId, int quantity, CancellationToken cancellationToken);

    /// <summary>
    /// Удаляет позицию из корзины.
    /// </summary>
    /// <param name="productId">Идентификатор товара.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Обновлённая корзина.</returns>
    Task<CartView> RemoveItemAsync(string productId, CancellationToken cancellationToken);

    /// <summary>
    /// Удаляет все позиции из корзины.
    /// </summary>
    void Clear();
}
