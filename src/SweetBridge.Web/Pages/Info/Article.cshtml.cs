using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using SweetBridge.Web.Services.Content;

namespace SweetBridge.Web.Pages.Info;

/// <summary>
/// Информационная страница: доставка, оплата, документы и статьи о проекте.
/// </summary>
public class ArticleModel(IContentService contentService) : PageModel
{
    /// <summary>
    /// Адрес статьи.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Slug { get; set; }

    /// <summary>
    /// Статья.
    /// </summary>
    public ContentPage? Article { get; private set; }

    /// <summary>
    /// Остальные статьи раздела.
    /// </summary>
    public IReadOnlyList<ContentPage> Related { get; private set; } = [];

    /// <summary>
    /// Загружает статью.
    /// </summary>
    /// <returns>Страница статьи или 404.</returns>
    public IActionResult OnGet()
    {
        Article = contentService.Find(Slug);

        if (Article is null)
        {
            return NotFound();
        }

        Related = [.. contentService.GetAll().Where(page => page.Slug != Article.Slug).Take(6)];

        return Page();
    }
}