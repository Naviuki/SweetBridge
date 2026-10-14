using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Избранный товар пользователя.
/// </summary>
public sealed class Favorite
{
    private Favorite()
    {
    }

    private Favorite(string userId, string productId, DateTime createdAtUtc)
    {
        UserId = userId;
        ProductId = productId;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Идентификатор записи.
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
    /// Момент добавления, в UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Добавляет товар в избранное.
    /// </summary>
    /// <param name="userId">Пользователь.</param>
    /// <param name="productId">Товар.</param>
    /// <param name="createdAtUtc">Момент добавления.</param>
    /// <returns>Новая запись избранного.</returns>
    /// <exception cref="DomainException">Не заполнены идентификаторы.</exception>
    public static Favorite Create(string userId, string productId, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(productId))
        {
            throw new DomainException("Избранное можно добавить только авторизованному пользователю.");
        }

        return new Favorite(userId, productId, createdAtUtc);
    }
}