using System.Text;
using Microsoft.AspNetCore.Html;

namespace SweetBridge.Web.Extensions;

/// <summary>
/// Renders icons from the local SVG sprite so that views stay free of repeated markup.
/// </summary>
public static class IconRenderer
{
    private const string SpritePath = "/icons/sprite.svg";

    /// <summary>
    /// Builds an inline SVG reference to the local sprite.
    /// </summary>
    /// <param name="name">Icon name without the <c>icon-</c> prefix.</param>
    /// <param name="cssClass">CSS class of the icon element.</param>
    /// <returns>HTML with the icon.</returns>
    public static IHtmlContent Render(string name, string cssClass = "icon")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var builder = new StringBuilder();
        builder.Append("<svg class=\"").Append(cssClass).Append("\" aria-hidden=\"true\"><use href=\"")
            .Append(SpritePath)
            .Append("#icon-")
            .Append(name)
            .Append("\"></use></svg>");

        return new HtmlString(builder.ToString());
    }
}