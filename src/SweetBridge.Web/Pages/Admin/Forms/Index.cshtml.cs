using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Services.Forms;

namespace SweetBridge.Web.Pages.Admin.Forms;

/// <summary>
/// Обращения, корпоративные заявки и подписчики.
/// </summary>
/// <param name="formsService">Сервис форм.</param>
public sealed class IndexModel(IFormsService formsService) : PageModel
{
    /// <summary>
    /// Обращения в поддержку.
    /// </summary>
    public IReadOnlyList<SupportRequestSummary> SupportRequests { get; private set; } = [];

    /// <summary>
    /// Корпоративные заявки.
    /// </summary>
    public IReadOnlyList<CorporateRequestSummary> CorporateRequests { get; private set; } = [];

    /// <summary>
    /// Количество подписчиков рассылки.
    /// </summary>
    public int Subscribers { get; private set; }

    /// <summary>
    /// Загружает формы.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        FormsOverview overview = await formsService.GetOverviewAsync(cancellationToken);

        SupportRequests = overview.SupportRequests;
        CorporateRequests = overview.CorporateRequests;
        Subscribers = overview.SubscriberCount;
    }

    /// <summary>
    /// Меняет статус обращения.
    /// </summary>
    /// <param name="id">Идентификатор обращения.</param>
    /// <param name="status">Новый статус.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на страницу форм.</returns>
    public async Task<IActionResult> OnPostSupportStatusAsync(int id, string status, CancellationToken cancellationToken)
    {
        await formsService.ChangeStatusAsync(FormEntityKind.SupportRequest, id, status, cancellationToken);

        return RedirectToPage();
    }

    /// <summary>
    /// Меняет статус корпоративной заявки.
    /// </summary>
    /// <param name="id">Идентификатор заявки.</param>
    /// <param name="status">Новый статус.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на страницу форм.</returns>
    public async Task<IActionResult> OnPostCorporateStatusAsync(int id, string status, CancellationToken cancellationToken)
    {
        await formsService.ChangeStatusAsync(FormEntityKind.CorporateRequest, id, status, cancellationToken);

        return RedirectToPage();
    }
}