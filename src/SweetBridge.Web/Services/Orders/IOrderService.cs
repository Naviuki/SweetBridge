using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Orders;

/// <summary>
/// Жизненный цикл заказа: оформление, оплата, отслеживание и администрирование.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Размещает заказ в волне: цены, вес, скидки и вместимость пересчитываются на сервере.
    /// </summary>
    /// <param name="command">Данные оформления заказа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданный заказ.</returns>
    Task<OrderSummary> PlaceAsync(PlaceOrderCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Находит заказ по номеру и телефону покупателя или получателя.
    /// </summary>
    /// <param name="number">Номер заказа.</param>
    /// <param name="phone">Номер телефона.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Заказ или <see langword="null"/>, если ничего не найдено.</returns>
    Task<OrderSummary?> FindAsync(string number, string phone, CancellationToken cancellationToken);

    /// <summary>
    /// Находит заказ по номеру без проверки телефона: используется на странице подтверждения
    /// сразу после оформления заказа покупателем.
    /// </summary>
    /// <param name="number">Номер заказа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Заказ или <see langword="null"/>, если ничего не найдено.</returns>
    Task<OrderSummary?> FindByNumberAsync(string number, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает заказы пользователя из личного кабинета.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Заказы пользователя.</returns>
    Task<IReadOnlyList<OrderSummary>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает заказы, подходящие под фильтр списка админки.
    /// </summary>
    /// <param name="filter">Фильтр по статусу и волне.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Подходящие заказы.</returns>
    Task<IReadOnlyList<OrderSummary>> GetOrdersAsync(OrderFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Переводит заказ на следующий статус жизненного цикла.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Обновлённый заказ.</returns>
    Task<OrderSummary> AdvanceStatusAsync(int orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Фиксирует успешную оплату заказа.
    /// </summary>
    /// <param name="orderNumber">Номер заказа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Оплаченный заказ.</returns>
    Task<OrderSummary> MarkPaidAsync(string orderNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Отменяет заказ покупателем и освобождает вес в волне.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Отменённый заказ.</returns>
    Task<OrderSummary> CancelByCustomerAsync(int orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Отменяет заказ администратором и освобождает вес в волне.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Отменённый заказ.</returns>
    Task<OrderSummary> CancelByAdminAsync(int orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Отменяет заказы с истёкшим резервом оплаты и освобождает вес в волнах.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество отменённых заказов.</returns>
    Task<int> ReleaseExpiredOrdersAsync(CancellationToken cancellationToken);
}