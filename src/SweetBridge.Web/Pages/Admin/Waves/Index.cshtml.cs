using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Auditing;
using SweetBridge.Web.Services.Waves;

namespace SweetBridge.Web.Pages.Admin.Waves;

/// <summary>
/// Управление волнами: создание и отправка.
/// </summary>
/// <param name="waveService">Сервис волн.</param>
/// <param name="auditService">Журнал действий.</param>
public sealed class IndexModel(IWaveService waveService, IAuditService auditService) : PageModel
{
    /// <summary>
    /// Все волны.
    /// </summary>
    public IReadOnlyList<WaveSummary> Waves { get; private set; } = [];

    /// <summary>
    /// Дата отправки новой волны.
    /// </summary>
    [BindProperty]
    [Required]
    public DateOnly ShipDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7);

    /// <summary>
    /// Направление новой волны.
    /// </summary>
    [BindProperty]
    public ShippingDirection Direction { get; set; } = ShippingDirection.YakutiaToDonbass;

    /// <summary>
    /// Загружает волны.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Waves = await waveService.GetAllAsync(cancellationToken);

    /// <summary>
    /// Создаёт волну.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на список волн.</returns>
    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        try
        {
            WaveSummary wave = await waveService.CreateAsync(new CreateWaveCommand(Direction, ShipDate), cancellationToken);

            await auditService.WriteAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system",
                "Wave.Create",
                nameof(Wave),
                $"Волна №{wave.Number} на {wave.ShipDate:yyyy-MM-dd}",
                cancellationToken);

            TempData["Success"] = $"Волна №{wave.Number} создана: отправка {wave.ShipDate:dd.MM.yyyy}.";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Отправляет волну.
    /// </summary>
    /// <param name="id">Идентификатор волны.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на список волн.</returns>
    public async Task<IActionResult> OnPostDispatchAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await waveService.DispatchAsync(id, cancellationToken);

            await auditService.WriteAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system",
                "Wave.Dispatch",
                nameof(Wave),
                $"Волна {id} отправлена",
                cancellationToken);

            TempData["Success"] = "Волна отправлена, заказы переведены в статус «В пути».";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage();
    }
}