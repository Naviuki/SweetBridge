using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Services.Notifications;

namespace SweetBridge.Web.Pages.Account;

/// <summary>
/// Уведомления покупателя о статусах заказов.
/// </summary>
/// <param name="notificationService">Сервис уведомлений.</param>
public sealed class NotificationsModel(INotificationService notificationService) : PageModel
{
    /// <summary>
    /// Уведомления пользователя.
    /// </summary>
    public IReadOnlyList<NotificationSummary> Notifications { get; private set; } = [];

    /// <summary>
    /// Количество непрочитанных уведомлений.
    /// </summary>
    public int UnreadCount { get; private set; }

    /// <summary>
    /// Загружает уведомления.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        Notifications = await notificationService.GetAllAsync(userId, cancellationToken);
        UnreadCount = await notificationService.GetUnreadCountAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Помечает уведомления прочитанными.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на страницу уведомлений.</returns>
    public async Task<IActionResult> OnPostMarkReadAsync(CancellationToken cancellationToken)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await notificationService.MarkAllAsReadAsync(userId, cancellationToken);
            TempData["Success"] = "Уведомления отмечены прочитанными.";
        }

        return RedirectToPage();
    }
}