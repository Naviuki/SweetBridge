using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Infrastructure.Persistence;

namespace SweetBridge.Web.Pages.Admin.Users;

/// <summary>
/// Список пользователей платформы.
/// </summary>
/// <param name="dbContext">Контекст базы данных.</param>
/// <param name="userManager">Менеджер пользователей Identity.</param>
public sealed class IndexModel(AppDbContext dbContext, UserManager<AppUser> userManager) : PageModel
{
    /// <summary>
    /// Пользователи.
    /// </summary>
    public IReadOnlyList<UserRow> Users { get; private set; } = [];

    /// <summary>
    /// Загружает пользователей.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        List<AppUser> users = await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        Dictionary<string, int> orderCounts = await dbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId != null)
            .GroupBy(order => order.UserId!)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken);

        List<string> adminIds = [.. (await userManager.GetUsersInRoleAsync("Admin")).Select(user => user.Id)];

        Users =
        [
            .. users.Select(user => new UserRow(
                user.Id,
                user.FullName,
                user.Email ?? string.Empty,
                user.PhoneNumberNormalized ?? string.Empty,
                user.CreatedAtUtc,
                user.IsBlocked,
                adminIds.Contains(user.Id),
                orderCounts.GetValueOrDefault(user.Id, 0)))
        ];
    }

    /// <summary>
    /// Блокирует или разблокирует пользователя.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на страницу пользователей.</returns>
    public async Task<IActionResult> OnPostToggleBlockAsync(string id, CancellationToken cancellationToken)
    {
        AppUser? user = await dbContext.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        if (await userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["Error"] = "Блокировать администратора нельзя.";

            return RedirectToPage();
        }

        user.IsBlocked = !user.IsBlocked;

        if (user.IsBlocked)
        {
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(10));
        }
        else
        {
            await userManager.SetLockoutEndDateAsync(user, null);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        TempData["Info"] = user.IsBlocked ? "Доступ пользователю заблокирован." : "Пользователь разблокирован.";

        return RedirectToPage();
    }

    /// <summary>
    /// Строка списка пользователей.
    /// </summary>
    /// <param name="Id">Идентификатор.</param>
    /// <param name="FullName">Имя.</param>
    /// <param name="Email">Почта.</param>
    /// <param name="Phone">Телефон.</param>
    /// <param name="CreatedAtUtc">Момент регистрации.</param>
    /// <param name="IsBlocked">Признак блокировки.</param>
    /// <param name="IsAdmin">Признак администратора.</param>
    /// <param name="OrdersCount">Количество заказов.</param>
    public sealed record UserRow(
        string Id,
        string FullName,
        string Email,
        string Phone,
        DateTime CreatedAtUtc,
        bool IsBlocked,
        bool IsAdmin,
        int OrdersCount);
}