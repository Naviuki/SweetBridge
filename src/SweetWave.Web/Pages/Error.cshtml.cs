using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SweetWave.Web.Pages;

/// <summary>
/// Friendly page for 404 and 500 responses.
/// </summary>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public sealed class ErrorModel : PageModel
{
    /// <summary>
    /// Request identifier shown to the user.
    /// </summary>
    public string? RequestId { get; private set; }

    /// <summary>
    /// Indicates that the request identifier is known.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    /// <summary>
    /// Handles the error response.
    /// </summary>
    /// <param name="requestId">Identifier of the failed request.</param>
    /// <returns>Task representing the operation.</returns>
    public IActionResult OnGet(string? requestId)
    {
        RequestId = Activity.Current?.Id ?? requestId;

        return Page();
    }
}