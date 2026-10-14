using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Services.Auditing;

namespace SweetBridge.Web.Pages.Admin.PromoCodes;

/// <summary>
/// Управление промокодами.
/// </summary>
/// <param name="dbContext">Контекст базы данных.</param>
/// <param name="auditService">Журнал действий.</param>
public sealed class IndexModel(AppDbContext dbContext, IAuditService auditService) : PageModel
{
    /// <summary>
    /// Промокоды.
    /// </summary>
    public IReadOnlyList<PromoCode> PromoCodes { get; private set; } = [];

    /// <summary>
    /// Данные нового промокода.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Загружает промокоды.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Input.ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow);
        Input.ValidTo = Input.ValidFrom.AddDays(30);

        PromoCodes = await dbContext.PromoCodes
            .AsNoTracking()
            .OrderByDescending(code => code.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Создаёт промокод.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на страницу промокодов.</returns>
    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToPage();
        }

        try
        {
            PromoCode promo = PromoCode.Create(
                Input.Code,
                Input.Kind,
                Input.Value,
                Input.ValidFrom,
                Input.ValidTo,
                Input.MaxUses,
                Input.MinOrderAmount);

            dbContext.PromoCodes.Add(promo);
            await dbContext.SaveChangesAsync(cancellationToken);

            await auditService.WriteAsync(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system",
                "PromoCode.Create",
                nameof(PromoCode),
                $"Промокод {promo.Code}",
                cancellationToken);

            TempData["Success"] = $"Промокод {promo.Code} создан.";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Включает или выключает промокод.
    /// </summary>
    /// <param name="id">Идентификатор промокода.</param>
    /// <param name="isActive">Требуемая активность.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на страницу промокодов.</returns>
    public async Task<IActionResult> OnPostToggleAsync(int id, bool isActive, CancellationToken cancellationToken)
    {
        PromoCode? promo = await dbContext.PromoCodes.FirstOrDefaultAsync(code => code.Id == id, cancellationToken);

        if (promo is not null)
        {
            if (isActive)
            {
                promo.Activate();
            }
            else
            {
                promo.Deactivate();
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            await auditService.WriteAsync(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system",
                "PromoCode.Toggle",
                nameof(PromoCode),
                $"Промокод {promo.Code}",
                cancellationToken);
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Данные формы промокода.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Код промокода.
        /// </summary>
        [Required(ErrorMessage = "Укажите код промокода.")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Код должен быть от 3 до 32 символов.")]
        [Display(Name = "Код")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Тип скидки.
        /// </summary>
        [Display(Name = "Тип скидки")]
        public PromoCodeKind Kind { get; set; } = PromoCodeKind.Percent;

        /// <summary>
        /// Размер скидки.
        /// </summary>
        [Range(0.01, 1_000_000, ErrorMessage = "Размер скидки должен быть больше нуля.")]
        [Display(Name = "Размер")]
        public decimal Value { get; set; }

        /// <summary>
        /// Начало действия.
        /// </summary>
        [Display(Name = "Действует с")]
        public DateOnly ValidFrom { get; set; }

        /// <summary>
        /// Окончание действия.
        /// </summary>
        [Display(Name = "Действует по")]
        public DateOnly ValidTo { get; set; }

        /// <summary>
        /// Лимит применений, 0 — без ограничений.
        /// </summary>
        [Range(0, 1_000_000, ErrorMessage = "Лимит применений не может быть отрицательным.")]
        [Display(Name = "Лимит применений")]
        public int MaxUses { get; set; }

        /// <summary>
        /// Минимальная сумма товаров.
        /// </summary>
        [Range(0, 1_000_000, ErrorMessage = "Минимальная сумма не может быть отрицательной.")]
        [Display(Name = "Минимальная сумма, ₽")]
        public decimal MinOrderAmount { get; set; }
    }
}