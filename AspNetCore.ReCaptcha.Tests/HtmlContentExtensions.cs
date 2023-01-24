using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace AspNetCore.ReCaptcha.Tests;

/// <summary>
/// Extension methods for <see cref="IHtmlContent"/>.
/// </summary>
public static class HtmlContentExtensions
{
    /// <summary>
    /// Returns the string representation of the HTML content.
    /// </summary>
    /// <param name="content">HTML content which can be written to a TextWriter.</param>
    /// <returns>The string representation of the HTML content.</returns>
    public static string ToHtmlString(this IHtmlContent content)
    {
        if (content is HtmlString htmlString)
        {
            return htmlString.Value;
        }

        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}