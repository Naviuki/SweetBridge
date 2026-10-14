using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Waves;

namespace SweetBridge.Web.Pages;

/// <summary>
/// Главная страница: обе витрины, ближайшие волны и краткое описание механики.
/// </summary>
/// <param name="waveService">Сервис волн.</param>
/// <param name="catalogService">Сервис каталога.</param>
public sealed class IndexModel(IWaveService waveService, ICatalogService catalogService) : PageModel
{
    private const int NearestWavesCount = 2;
    private const int ShowcaseCount = 4;

    /// <summary>
    /// Ближайшие волны со сладостями Якутии.
    /// </summary>
    public IReadOnlyList<WaveSummary> WavesToDonbass { get; private set; } = [];

    /// <summary>
    /// Ближайшие волны со сладостями Донбасса.
    /// </summary>
    public IReadOnlyList<WaveSummary> WavesToYakutia { get; private set; } = [];

    /// <summary>
    /// Популярные товары витрины Якутии.
    /// </summary>
    public IReadOnlyList<ProductCardViewModel> YakutiaShowcase { get; private set; } = [];

    /// <summary>
    /// Популярные товары витрины Донбасса.
    /// </summary>
    public IReadOnlyList<ProductCardViewModel> DonbassShowcase { get; private set; } = [];

    /// <summary>
    /// Загружает ближайшие волны обоих направлений и витрины.
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

        YakutiaShowcase = ToCards(await catalogService.GetShowcaseAsync(
            ShippingDirection.YakutiaToDonbass,
            ShowcaseCount,
            cancellationToken), Region.Yakutia);

        DonbassShowcase = ToCards(await catalogService.GetShowcaseAsync(
            ShippingDirection.DonbassToYakutia,
            ShowcaseCount,
            cancellationToken), Region.Donbass);
    }

    private static IReadOnlyList<ProductCardViewModel> ToCards(IReadOnlyList<ProductSummary> products, Region region) =>
        [.. products.Select(product => new ProductCardViewModel(
            product.Id,
            product.Name,
            product.Category,
            product.ProducerName,
            region,
            product.WeightGrams,
            product.Price,
            product.IsTemperatureSensitive,
            product.PhotoPath,
            QuantityInCart: 0))];
}