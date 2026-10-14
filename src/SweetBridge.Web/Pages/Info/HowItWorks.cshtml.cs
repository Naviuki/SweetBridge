using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using SweetBridge.Web.Options;
using SweetBridge.Web.Services.Content;

namespace SweetBridge.Web.Pages.Info;

/// <summary>
/// Страница «Как работает волна»: шаги доставки, скидки, дедлайны и термоупаковка.
/// </summary>
/// <param name="contentService">Сервис информационных страниц.</param>
/// <param name="logistics">Логистические параметры.</param>
public class HowItWorksModel(IContentService contentService, IOptions<LogisticsOptions> logistics) : PageModel
{
    private readonly ContentPage _page = contentService.Find("how-it-works")
        ?? throw new InvalidOperationException("Статья «how-it-works» не найдена в контенте.");

    /// <summary>
    /// Шаги доставки.
    /// </summary>
    [BindNever]
    public IReadOnlyList<ContentBlock> Steps => _page.Blocks;

    /// <summary>
    /// Интервал между волнами в днях.
    /// </summary>
    [BindNever]
    public int WaveIntervalDays => logistics.Value.WaveIntervalDays;

    /// <summary>
    /// Пороги скидок за заполнение волны.
    /// </summary>
    [BindNever]
    public IReadOnlyList<FillDiscountOption> FillDiscounts => logistics.Value.FillDiscounts;

    /// <summary>
    /// Минимальная загрузка волны в килограммах.
    /// </summary>
    [BindNever]
    public int MinWaveLoadKg => logistics.Value.MinWaveLoadKg;

    /// <summary>
    /// Минимальный срок доставки в днях.
    /// </summary>
    [BindNever]
    public int MinDeliveryDays => logistics.Value.MinDeliveryDays;

    /// <summary>
    /// Максимальный срок доставки в днях.
    /// </summary>
    [BindNever]
    public int MaxDeliveryDays => logistics.Value.MaxDeliveryDays;

    /// <summary>
    /// За сколько дней до отправки закрывается приём заказов.
    /// </summary>
    [BindNever]
    public int CutoffDays => logistics.Value.OrderCutoffDaysBeforeShipment;
}