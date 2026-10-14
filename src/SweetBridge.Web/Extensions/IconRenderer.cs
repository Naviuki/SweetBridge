using System.Text;
using Microsoft.AspNetCore.Html;

namespace SweetBridge.Web.Extensions;

/// <summary>
/// Формирует ссылку на символ инлайн-спрайта иконок, вставленного в <c>_Layout</c>.
/// Внешний спрайт не используется: он не работал в браузере из-за политики CORS.
/// </summary>
public static class IconRenderer
{
    /// <summary>
    /// Возвращает разметку иконки.
    /// </summary>
    /// <param name="name">Имя символа без префикса <c>icon-</c>.</param>
    /// <param name="cssClass">Класс элемента иконки.</param>
    /// <returns>HTML со ссылкой на символ спрайта.</returns>
    public static IHtmlContent Render(string name, string cssClass = "icon")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var builder = new StringBuilder();
        builder.Append("<svg class=\"")
            .Append(cssClass)
            .Append("\" aria-hidden=\"true\" focusable=\"false\"><use href=\"#icon-")
            .Append(name)
            .Append("\"></use></svg>");

        return new HtmlString(builder.ToString());
    }
}