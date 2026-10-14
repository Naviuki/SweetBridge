using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Services.Catalog;

namespace SweetBridge.Web.Pages.Admin.Products;

/// <summary>
/// Список товаров администратора.
/// </summary>
/// <param name="productService">Сервис управления каталогом.</param>
public sealed class IndexModel(IProductManagementService productService) : PageModel
{
    /// <summary>
    /// Все товары, включая скрытые.
    /// </summary>
    public IReadOnlyList<ProductSummary> Products { get; private set; } = [];

    /// <summary>
    /// Загружает товары.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Products = await productService.GetAllAsync(cancellationToken);

    /// <summary>
    /// Показывает или скрывает товар.
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <param name="isVisible">Требуемая видимость.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на список товаров.</returns>
    public async Task<IActionResult> OnPostVisibilityAsync(
        string id,
        bool isVisible,
        CancellationToken cancellationToken)
    {
        await productService.SetVisibilityAsync(id, isVisible, cancellationToken);
        TempData["Info"] = isVisible ? "Товар снова в каталоге." : "Товар скрыт из каталога.";

        return RedirectToPage();
    }
}