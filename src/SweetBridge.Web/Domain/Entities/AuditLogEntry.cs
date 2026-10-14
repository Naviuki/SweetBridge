using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Запись журнала действий администратора без персональных данных и секретов.
/// </summary>
public sealed class AuditLogEntry
{
    private AuditLogEntry()
    {
    }

    private AuditLogEntry(string userId, string action, string targetType, string? details, DateTime createdAtUtc)
    {
        UserId = userId;
        Action = action;
        TargetType = targetType;
        Details = details;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Идентификатор администратора.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Действие, например <c>Wave.Dispatch</c>.
    /// </summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>
    /// Тип изменённого объекта.
    /// </summary>
    public string TargetType { get; private set; } = string.Empty;

    /// <summary>
    /// Дополнительные сведения без секретов и персональных данных.
    /// </summary>
    public string? Details { get; private set; }

    /// <summary>
    /// Момент действия, в UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Создаёт запись журнала.
    /// </summary>
    /// <param name="userId">Администратор.</param>
    /// <param name="action">Действие.</param>
    /// <param name="targetType">Тип объекта.</param>
    /// <param name="details">Сведения.</param>
    /// <param name="createdAtUtc">Момент действия.</param>
    /// <returns>Новая запись журнала.</returns>
    /// <exception cref="DomainException">Обязательные поля не заполнены.</exception>
    public static AuditLogEntry Create(
        string userId,
        string action,
        string targetType,
        string? details,
        DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(action) || string.IsNullOrWhiteSpace(targetType))
        {
            throw new DomainException("Запись журнала должна содержать пользователя, действие и тип объекта.");
        }

        return new AuditLogEntry(userId, action.Trim(), targetType.Trim(), details, createdAtUtc);
    }
}