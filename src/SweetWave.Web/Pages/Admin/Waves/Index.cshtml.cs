using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Extensions;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Services.Waves;

namespace SweetWave.Web.Pages.Admin.Waves;

/// <summary>
/// Wave schedule: list with fill, creation of a wave and dispatching.
/// </summary>
/// <param name="waveService">Wave service.</param>
public sealed class IndexModel(IWaveService waveService) : PageModel
{
    /// <summary>
    /// Data of the wave creation form.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// All waves of both directions.
    /// </summary>
    public IReadOnlyList<WaveSummary> Waves { get; private set; } = [];

    /// <summary>
    /// Loads the wave schedule.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Waves = await waveService.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a planned wave with an automatically calculated order cutoff date.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect back to the list.</returns>
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
    /// Dispatches the wave and moves its orders to the in transit status.
    /// </summary>
    /// <param name="waveId">Wave identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect back to the list.</returns>
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
    /// Data of the wave creation form.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Delivery direction.
        /// </summary>
        [Display(Name = "Направление")]
        public ShippingDirection Direction { get; set; } = ShippingDirection.YakutiaToDonbass;

        /// <summary>
        /// Planned shipment date.
        /// </summary>
        [Required(ErrorMessage = "Укажите дату отправки.")]
        [Display(Name = "Дата отправки")]
        public DateOnly? ShipDate { get; set; }
    }
}