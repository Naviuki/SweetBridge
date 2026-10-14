using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Services.Common;

namespace SweetBridge.Web.Services.Notifications;

/// <summary>
/// Уведомления пользователей о событиях заказа.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Создаёт уведомление.
    /// </summary>
    /// <param name="userId">Получатель.</param>
    /// <param name="title">Заголовок.</param>
    /// <param name="body">Текст.</param>
    /// <param name="orderId">Связанный заказ.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task AddAsync(string userId, string title, string body, int? orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает уведомления пользователя, новые сверху.
    /// </summary>
    /// <param name="userId">Получатель.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список уведомлений.</returns>
    Task<IReadOnlyList<NotificationSummary>> GetAllAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает количество непрочитанных уведомлений.
    /// </summary>
    /// <param name="userId">Получатель.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество непрочитанных уведомлений.</returns>
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Помечает уведомления прочитанными.
    /// </summary>
    /// <param name="userId">Получатель.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken);
}

/// <summary>
/// Модель чтения уведомления.
/// </summary>
/// <param name="Id">Идентификатор уведомления.</param>
/// <param name="Title">Заголовок.</param>
/// <param name="Body">Текст.</param>
/// <param name="OrderId">Связанный заказ.</param>
/// <param name="IsRead">Признак прочитанного.</param>
/// <param name="CreatedAtUtc">Момент создания.</param>
public sealed record NotificationSummary(
    int Id,
    string Title,
    string Body,
    int? OrderId,
    bool IsRead,
    DateTime CreatedAtUtc);

/// <summary>
/// Уведомления поверх базы данных.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly AppDbContext _dbContext;
    private readonly IBusinessClock _clock;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="NotificationService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="clock">Источник времени.</param>
    public NotificationService(AppDbContext dbContext, IBusinessClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(clock);

        _dbContext = dbContext;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task AddAsync(string userId, string title, string body, int? orderId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        _dbContext.Notifications.Add(Notification.Create(userId, title, body, orderId, _clock.UtcNow));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationSummary>> GetAllAsync(string userId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .Take(50)
            .Select(notification => new NotificationSummary(
                notification.Id,
                notification.Title,
                notification.Body,
                notification.OrderId,
                notification.IsRead,
                notification.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return 0;
        }

        return await _dbContext.Notifications
            .AsNoTracking()
            .CountAsync(notification => notification.UserId == userId && !notification.IsRead, cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken)
    {
        var unread = await _dbContext.Notifications
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
        {
            return;
        }

        foreach (Notification notification in unread)
        {
            notification.MarkAsRead();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}