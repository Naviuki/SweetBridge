using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Services.Waves;

namespace SweetWave.Web.Pages;

/// <summary>
/// Home page: both showcases, nearest waves and a short description of the mechanics.
/// </summary>
/// <param name="waveService">Wave service.</param>
public sealed class IndexModel(IWaveService waveService) : PageModel
{
    private const int NearestWavesCount = 2;

    /// <summary>
    /// Nearest waves with the Yakutia confectionery.
    /// </summary>
    public IReadOnlyList<WaveSummary> WavesToDonbass { get; private set; } = [];

    /// <summary>
    /// Nearest waves with the Donbass confectionery.
    /// </summary>
    public IReadOnlyList<WaveSummary> WavesToYakutia { get; private set; } = [];

    /// <summary>
    /// Loads the nearest waves of both directions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        WavesToDonbass = await waveService.GetUpcomingAsync(
            ShippingDirection.YakutiaToDonbass,
            NearestWavesCount,
            cancellationToken);

        WavesToYakutia = await waveService.GetUpcomingAsync(
            ShippingDirection.DonbassToYakutia,
            NearestWavesCount,
            cancellationToken);
    }
}