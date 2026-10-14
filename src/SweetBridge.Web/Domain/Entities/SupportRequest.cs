using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Обращение пользователя в поддержку.
/// </summary>
public sealed class SupportRequest
{
    private SupportRequest()
    {
    }

    private SupportRequest(string name, string email, string? orderNumber, string message, DateTime createdAtUtc)
    {
        Name = name;
        Email = email;
        OrderNumber = orderNumber;
        Message = message;
        CreatedAtUtc = createdAtUtc;
        Status = Enums.SupportRequestStatus.New;
    }

    /// <summary>
    /// Идентификатор обращения.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Имя отправителя.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Почта отправителя.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Номер связанного заказа.
    /// </summary>
    public string? OrderNumber { get; private set; }

    /// <summary>
    /// Текст обращения.
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// Статус обращения.
    /// </summary>
    public Enums.SupportRequestStatus Status { get; private set; }

    /// <summary>
    /// Момент создания, в UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Создаёт обращение.
    /// </summary>
    /// <param name="name">Имя.</param>
    /// <param name="email">Почта.</param>
    /// <param name="orderNumber">Номер заказа.</param>
    /// <param name="message">Текст обращения.</param>
    /// <param name="createdAtUtc">Момент создания.</param>
    /// <returns>Новое обращение.</returns>
    /// <exception cref="DomainException">Обязательные поля не заполнены.</exception>
    public static SupportRequest Create(string name, string email, string? orderNumber, string message, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
        {
            throw new DomainException("Заполните имя, почту и текст обращения.");
        }

        return new SupportRequest(name.Trim(), email.Trim(), orderNumber?.Trim(), message.Trim(), createdAtUtc);
    }

    /// <summary>
    /// Меняет статус обращения.
    /// </summary>
    /// <param name="status">Новый статус.</param>
    public void ChangeStatus(Enums.SupportRequestStatus status) => Status = status;
}