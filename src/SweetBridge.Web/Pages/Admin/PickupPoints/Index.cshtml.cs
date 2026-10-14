using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Infrastructure.Persistence;

namespace SweetBridge.Web.Pages.Admin.PickupPoints;

/// <summary>
/// Управление пунктами выдачи.
/// </summary>
/// <param name="dbContext">Контекст базы данных.</param>
public sealed class IndexModel(AppDbContext dbContext) : PageModel
{
    /// <summary>
    /// Пункты выдачи.
    /// </summary>
    public IReadOnlyList<PickupPoint> Points { get; private set; } = [];

    /// <summary>
    /// Данные нового пункта выдачи.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Загружает пункты выдачи.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Points = await dbContext.PickupPoints
            .AsNoTracking()
            .OrderBy(point => point.Region)
            .ThenBy(point => point.City)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Создаёт пункт выдачи.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на страницу пунктов выдачи.</returns>
    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToPage();
        }

        try
        {
            dbContext.PickupPoints.Add(PickupPoint.Create(
                Input.Region,
                Input.City,
                Input.Number,
                Input.Address));

            await dbContext.SaveChangesAsync(cancellationToken);
            TempData["Success"] = "Пункт выдачи добавлен.";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Показывает или скрывает пункт выдачи.
    /// </summary>
    /// <param name="id">Идентификатор пункта выдачи.</param>
    /// <param name="isVisible">Требуемая видимость.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на страницу пунктов выдачи.</returns>
    public async Task<IActionResult> OnPostVisibilityAsync(int id, bool isVisible, CancellationToken cancellationToken)
    {
        PickupPoint? point = await dbContext.PickupPoints
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (point is not null)
        {
            if (isVisible)
            {
                point.Show();
            }
            else
            {
                point.Hide();
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Данные формы пункта выдачи.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Регион.
        /// </summary>
        [Display(Name = "Регион")]
        public Region Region { get; set; } = Region.Yakutia;

        /// <summary>
        /// Город.
        /// </summary>
        [Required(ErrorMessage = "Укажите город.")]
        [StringLength(200, ErrorMessage = "Город не должен быть длиннее 200 символов.")]
        [Display(Name = "Город")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Номер пункта выдачи.
        /// </summary>
        [Required(ErrorMessage = "Укажите номер пункта выдачи.")]
        [StringLength(50, ErrorMessage = "Номер пункта не должен быть длиннее 50 символов.")]
        [Display(Name = "Номер пункта")]
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Адрес.
        /// </summary>
        [Required(ErrorMessage = "Укажите адрес.")]
        [StringLength(300, ErrorMessage = "Адрес не должен быть длиннее 300 символов.")]
        [Display(Name = "Адрес")]
        public string Address { get; set; } = string.Empty;
    }
}