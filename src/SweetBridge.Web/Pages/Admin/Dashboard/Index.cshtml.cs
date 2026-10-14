using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Services.Forms;
using SweetBridge.Web.Services.Waves;

namespace SweetBridge.Web.Pages.Admin.Dashboard;

/// <summary>
/// Сводная панель администратора: заказы, волны, выручка и обращения.
/// </summary>
/// <param name="waveService">Сервис волн.</param>
/// <param name="formsService">Сервис форм.</param>
/// <param name="dbContext">Контекст базы данных.</param>
public sealed class IndexModel(
    IWaveService waveService,
    IFormsService formsService,
    AppDbContext dbContext) : PageModel
{
    /// <summary>
    /// Волны обоих направлений.
    /// </summary>
    public IReadOnlyList<WaveSummary> Waves { get; private set; } = [];

    /// <summary>
    /// Количество заказов.
    /// </summary>
    public int OrdersCount { get; private set; }

    /// <summary>
    /// Количество заказов, ожидающих оплаты.
    /// </summary>
    public int UnpaidCount { get; private set; }

    /// <summary>
    /// Количество заказов в пути.
    /// </summary>
    public int InTransitCount { get; private set; }

    /// <summary>
    /// Сумма оплаченных заказов в рублях.
    /// </summary>
    public decimal PaidTotal { get; private set; }

    /// <summary>
    /// Количество активных товаров.
    /// </summary>
    public int ProductsCount { get; private set; }

    /// <summary>
    /// Количество активных промокодов.
    /// </summary>
    public int PromoCodesCount { get; private set; }

    /// <summary>
    /// Количество новых обращений.
    /// </summary>
    public int NewSupportRequests { get; private set; }

    /// <summary>
    /// Количество новых корпоративных заявок.
    /// </summary>
    public int NewCorporateRequests { get; private set; }

    /// <summary>
    /// Количество подписчиков рассылки.
    /// </summary>
    public int Subscribers { get; private set; }

    /// <summary>
    /// Загружает сводку.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Waves = await waveService.GetAllAsync(cancellationToken);

        OrdersCount = await dbContext.Orders.CountAsync(cancellationToken);
        UnpaidCount = await dbContext.Orders
            .CountAsync(order => order.PaymentStatus == PaymentStatus.Pending, cancellationToken);
        InTransitCount = await dbContext.Orders
            .CountAsync(order => order.Status == OrderStatus.InTransit, cancellationToken);

        List<decimal?> paid = await dbContext.Orders
            .Where(order => order.PaymentStatus == PaymentStatus.Paid)
            .Select(order => (decimal?)order.GoodsCost + order.ShippingCost + order.ThermoPackCost - order.DiscountAmount)
            .ToListAsync(cancellationToken);

        PaidTotal = paid.Sum(value => value ?? 0m);
        ProductsCount = await dbContext.Products.CountAsync(product => product.IsActive, cancellationToken);
        PromoCodesCount = await dbContext.PromoCodes.CountAsync(code => code.IsActive, cancellationToken);

        FormsOverview overview = await formsService.GetOverviewAsync(cancellationToken);
        NewSupportRequests = overview.SupportRequests.Count(request => request.Status == SupportRequestStatus.New);
        NewCorporateRequests = overview.CorporateRequests.Count(request => request.Status == CorporateRequestStatus.New);
        Subscribers = overview.SubscriberCount;
    }
}