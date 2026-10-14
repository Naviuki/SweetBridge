using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Services.Gifts;

namespace SweetBridge.Web.Pages.Admin.GiftSets;

/// <summary>
/// Список подарочных наборов.
/// </summary>
/// <param name="giftSetService">Сервис подарочных наборов.</param>
public sealed class IndexModel(IGiftSetService giftSetService) : PageModel
{
    /// <summary>
    /// Наборы.
    /// </summary>
    public IReadOnlyList<GiftSetSummary> Sets { get; private set; } = [];

    /// <summary>
    /// Загружает наборы.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Sets = await giftSetService.GetAllAsync(cancellationToken);
}