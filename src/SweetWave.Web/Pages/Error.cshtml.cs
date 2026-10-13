using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SweetWave.Web.Pages;

/// <summary>
/// Понятная страница для ответов 404 и 500.
/// </summary>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public sealed class ErrorModel : PageModel
{
    /// <summary>
    /// Идентификатор запроса, показываемый пользователю.
    /// </summary>
    public string? RequestId { get; private set; }

    /// <summary>
    /// Признак того, что идентификатор запроса известен.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    /// <summary>
    /// Обрабатывает ответ с ошибкой.
    /// </summary>
    /// <param name="requestId">Идентификатор неуспешного запроса.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    public IActionResult OnGet(string? requestId)
    {
        RequestId = Activity.Current?.Id ?? requestId;

        return Page();
    }
}
