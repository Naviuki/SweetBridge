using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Catalog;

namespace SweetBridge.Web.Pages.Admin.Products;

/// <summary>
/// Управление каталогом: список всех товаров, включая скрытые.
/// </summary>
/// <param name="productManagementService">Сервис управления товарами.</param>
public sealed class IndexModel(IProductManagementService productManagementService) : PageModel
{
    /// <summary>
    /// Все товары каталога.
    /// </summary>
    public IReadOnlyList<ProductSummary> Products { get; private set; } = [];

    /// <summary>
    /// Загружает все товары.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Products = await productManagementService.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Скрывает товар или возвращает его в каталог.
    /// </summary>
    /// <param name="Id">Идентификатор товара.</param>
    /// <param name="isVisible">Требуемая видимость.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Возврат к списку.</returns>
    public async Task<IActionResult> OnPostVisibilityAsync(string id, bool isVisible, CancellationToken cancellationToken)
    {
        try
        {
            await productManagementService.SetVisibilityAsync(id, isVisible, cancellationToken);
            TempData["Success"] = isVisible
                ? $"Товар {id} снова в каталоге."
                : $"Товар {id} скрыт из каталога.";
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToPage();
    }
}
