using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Уведомление пользователя о событиях заказа.
/// </summary>
public sealed class Notification
{
    private Notification()
    {
    }

    private Notification(string userId, string title, string body, int? orderId, DateTime createdAtUtc)
    {
        UserId = userId;
        Title = title;
        Body = body;
        OrderId = orderId;
        CreatedAtUtc = createdAtUtc;
        IsRead = false;
    }

    /// <summary>
    /// Идентификатор уведомления.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Идентификатор получателя.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Заголовок уведомления.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Текст уведомления.
    /// </summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>
    /// Связанный заказ.
    /// </summary>
    public int? OrderId { get; private set; }

    /// <summary>
    /// Признак прочитанного сообщения.
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    /// Момент создания, в UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Создаёт уведомление.
    /// </summary>
    /// <param name="userId">Получатель.</param>
    /// <param name="title">Заголовок.</param>
    /// <param name="body">Текст.</param>
    /// <param name="orderId">Связанный заказ.</param>
    /// <param name="createdAtUtc">Момент создания.</param>
    /// <returns>Новое уведомление.</returns>
    /// <exception cref="DomainException">Получатель или текст не заполнены.</exception>
    public static Notification Create(string userId, string title, string body, int? orderId, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
        {
            throw new DomainException("Уведомление должно содержать получателя, заголовок и текст.");
        }

        return new Notification(userId, title.Trim(), body.Trim(), orderId, createdAtUtc);
    }

    /// <summary>
    /// Помечает уведомление прочитанным.
    /// </summary>
    public void MarkAsRead() => IsRead = true;
}