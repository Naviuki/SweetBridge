namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Генерирует уникальные номера заказов в формате <c>SW-XXXXXX</c>.
/// </summary>
public interface IOrderNumberGenerator
{
    /// <summary>
    /// Генерирует номер заказа, который ещё не используется.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Уникальный номер заказа.</returns>
    Task<string> GenerateAsync(CancellationToken cancellationToken);
}
