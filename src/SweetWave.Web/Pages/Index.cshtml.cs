using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Services.Waves;

namespace SweetWave.Web.Pages;

/// <summary>
/// Главная страница: обе витрины, ближайшие волны и краткое описание механики.
/// </summary>
/// <param name="waveService">Сервис волн.</param>
public sealed class IndexModel(IWaveService waveService) : PageModel
{
    private const int NearestWavesCount = 2;

    /// <summary>
    /// Ближайшие волны со сладостями Якутии.
    /// </summary>
    public IReadOnlyList<WaveSummary> WavesToDonbass { get; private set; } = [];

    /// <summary>
    /// Ближайшие волны со сладостями Донбасса.
    /// </summary>
    public IReadOnlyList<WaveSummary> WavesToYakutia { get; private set; } = [];

    /// <summary>
    /// Загружает ближайшие волны обоих направлений.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
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
