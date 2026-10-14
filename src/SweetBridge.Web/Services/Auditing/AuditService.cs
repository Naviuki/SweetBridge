using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Services.Common;

namespace SweetBridge.Web.Services.Auditing;

/// <summary>
/// Журнал действий администратора.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Записывает действие администратора.
    /// </summary>
    /// <param name="userId">Идентификатор администратора.</param>
    /// <param name="action">Действие, например <c>Wave.Dispatch</c>.</param>
    /// <param name="targetType">Тип объекта.</param>
    /// <param name="details">Сведения без персональных данных и секретов.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task WriteAsync(string userId, string action, string targetType, string? details, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает последние записи журнала.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Последние записи журнала.</returns>
    Task<IReadOnlyList<AuditEntrySummary>> GetRecentAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Модель чтения записи журнала.
/// </summary>
public sealed class AuditEntrySummary
{
    /// <summary>
    /// Действие.
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Тип объекта.
    /// </summary>
    public required string TargetType { get; init; }

    /// <summary>
    /// Сведения.
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Идентификатор администратора.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Почта администратора.
    /// </summary>
    public required string UserEmail { get; init; }

    /// <summary>
    /// Момент действия.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }
}

/// <summary>
/// Журнал действий поверх базы данных.
/// </summary>
public sealed class AuditService : IAuditService
{
    private readonly AppDbContext _dbContext;
    private readonly IBusinessClock _clock;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="AuditService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="clock">Источник времени.</param>
    public AuditService(AppDbContext dbContext, IBusinessClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(clock);

        _dbContext = dbContext;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task WriteAsync(
        string userId,
        string action,
        string targetType,
        string? details,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        _dbContext.AuditLogEntries.Add(
            AuditLogEntry.Create(userId, action, targetType, details, _clock.UtcNow));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AuditEntrySummary>> GetRecentAsync(CancellationToken cancellationToken)
    {
        Dictionary<string, string> userEmails = await _dbContext.Users
            .AsNoTracking()
            .ToDictionaryAsync(user => user.Id, user => user.Email ?? string.Empty, cancellationToken);

        var entries = await _dbContext.AuditLogEntries
            .AsNoTracking()
            .OrderByDescending(entry => entry.CreatedAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        return
        [
            .. entries.Select(entry => new AuditEntrySummary
            {
                Action = entry.Action,
                TargetType = entry.TargetType,
                Details = entry.Details,
                UserId = entry.UserId,
                UserEmail = userEmails.GetValueOrDefault(entry.UserId, entry.UserId),
                CreatedAtUtc = entry.CreatedAtUtc
            })
        ];
    }
}