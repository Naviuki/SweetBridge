using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Extensions;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Services.Waves;

namespace SweetWave.Web.Pages.Admin.Waves;

/// <summary>
/// Расписание волн: список с заполнением, создание волны и отправка.
/// </summary>
/// <param name="waveService">Сервис волн.</param>
public sealed class IndexModel(IWaveService waveService) : PageModel
{
    /// <summary>
    /// Данные формы создания волны.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Все волны обоих направлений.
    /// </summary>
    public IReadOnlyList<WaveSummary> Waves { get; private set; } = [];

    /// <summary>
    /// Загружает расписание волн.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Waves = await waveService.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Создаёт запланированную волну с автоматически рассчитанным дедлайном приёма заказов.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Возврат к списку.</returns>
    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (ModelState.IsValid && Input.ShipDate is DateOnly shipDate)
        {
            try
            {
                WaveSummary wave = await waveService.CreateAsync(
                    new CreateWaveCommand(Input.Direction, shipDate),
                    cancellationToken);

                TempData["Success"] = $"Волна на {wave.ShipDate.FormatDate()} создана, приём заказов до {wave.OrderCutoffDate.FormatDate()}.";
            }
            catch (DomainException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }
        }

        Waves = await waveService.GetAllAsync(cancellationToken);

        return Page();
    }

    /// <summary>
    /// Отправляет волну и переводит её заказы в статус «в пути».
    /// </summary>
    /// <param name="waveId">Идентификатор волны.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Возврат к списку.</returns>
    public async Task<IActionResult> OnPostDispatchAsync(int waveId, CancellationToken cancellationToken)
    {
        try
        {
            await waveService.DispatchAsync(waveId, cancellationToken);
            TempData["Success"] = "Волна отправлена, заказы переведены в статус «в пути».";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Данные формы создания волны.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Направление доставки.
        /// </summary>
        [Display(Name = "Направление")]
        public ShippingDirection Direction { get; set; } = ShippingDirection.YakutiaToDonbass;

        /// <summary>
        /// Запланированная дата отправки.
        /// </summary>
        [Required(ErrorMessage = "Укажите дату отправки.")]
        [Display(Name = "Дата отправки")]
        public DateOnly? ShipDate { get; set; }
    }
}
