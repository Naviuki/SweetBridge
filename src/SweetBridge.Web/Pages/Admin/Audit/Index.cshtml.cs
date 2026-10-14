using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Services.Auditing;

namespace SweetBridge.Web.Pages.Admin.Audit;

/// <summary>
/// Журнал действий администраторов.
/// </summary>
/// <param name="auditService">Журнал действий.</param>
public sealed class IndexModel(IAuditService auditService) : PageModel
{
    /// <summary>
    /// Последние записи журнала.
    /// </summary>
    public IReadOnlyList<AuditEntrySummary> Entries { get; private set; } = [];

    /// <summary>
    /// Загружает журнал.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Entries = await auditService.GetRecentAsync(cancellationToken);
}