using System;
using System.Net;
using System.Reflection.Metadata;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AspNetCore.ReCaptcha
{
    internal static class ReCaptchaGenerator
    {
        private static string ViewDataKey = "__ReCaptchaGeneratedId";

        public static int GenerateId(ViewContext viewContext)
        {
            var id = 0;
            if (viewContext.ViewData.TryGetValue(ViewDataKey, out var value) && value is int _id)
                id = _id;

            viewContext.ViewData[ViewDataKey] = ++id;
            return id;
        }

        /// <summary>
        /// Renders the Google ReCaptcha v2 HTML.
        /// </summary>
        /// <param name="baseUrl">The base URL where the Google Recaptcha JS script is hosted.</param>
        /// <param name="siteKey">The site key.</param>
        /// <param name="size">Optional parameter, contains the size of the widget.</param>
        /// <param name="theme">Google Recaptcha theme default is light.</param>
        /// <param name="language">Google Recaptcha <a href="https://developers.google.com/recaptcha/docs/language">Language Code</a></param>
        /// <param name="callback">Google ReCaptcha success callback method. Used in v2 ReCaptcha.</param>
        /// <param name="errorCallback">Google ReCaptcha error callback method. Used in v2 ReCaptcha.</param>
        /// <param name="expiredCallback">Google ReCaptcha expired callback method. Used in v2 ReCaptcha.</param>
        /// <param name="autoTheme">Indicates whether the theme is automatically set to 'dark' based on the user's system settings.</param>
        /// <returns></returns>
        public static IHtmlContent ReCaptchaV2(Uri baseUrl, string siteKey, string size, string theme, string language,
            string callback, string errorCallback, string expiredCallback, bool autoTheme = false, string nonce = null)
        {
            var content = new HtmlContentBuilder();
            content.AppendFormat(@"<div class=""g-recaptcha"" data-sitekey=""{0}""", siteKey);

            if (!string.IsNullOrEmpty(size))
                content.AppendFormat(@" data-size=""{0}""", size);
            if (!string.IsNullOrEmpty(theme))
                content.AppendFormat(@" data-theme=""{0}""", theme);
            if (!string.IsNullOrEmpty(callback))
                content.AppendFormat(@" data-callback=""{0}""", callback);
            if (!string.IsNullOrEmpty(errorCallback))
                content.AppendFormat(@" data-error-callback=""{0}""", errorCallback);
            if (!string.IsNullOrEmpty(expiredCallback))
                content.AppendFormat(@" data-expired-callback=""{0}""", expiredCallback);

            content.AppendFormat("></div>");
            content.AppendLine();
            content.AppendFormat(@"<script src=""{0}api.js?hl={1}""", baseUrl, language);
            content.AppendNonce(nonce, " defer></script>");

            if (autoTheme)
            {
                content
                    .AppendLine()
                    .AppendHtml("<script");

                if (!string.IsNullOrEmpty(nonce))
                    content.AppendFormat(" nonce=\"{0}\"", nonce);

                content.AppendHtmlLine(">window.matchMedia('(prefers-color-scheme: dark)').matches&&document.querySelector('.g-recaptcha').setAttribute('data-theme','dark');</script>");
            }

            return content;
        }

        public static IHtmlContent ReCaptchaV2Invisible(Uri baseUrl, string siteKey, string text, string className, string language, string callback, string errorCallback, string expiredCallback, string badge)
        {
            var content = new HtmlContentBuilder();
            content.AppendFormat(@"<button class=""g-recaptcha {0}""", className);
            content.AppendFormat(@" data-sitekey=""{0}""", siteKey);

            if (!string.IsNullOrEmpty(badge))
                content.AppendFormat(@" data-badge=""{0}""", badge);
            if (!string.IsNullOrEmpty(callback))
                content.AppendFormat(@" data-callback=""{0}""", callback);
            if (!string.IsNullOrEmpty(expiredCallback))
                content.AppendFormat(@" data-expired-callback=""{0}""", expiredCallback);
            if (!string.IsNullOrEmpty(errorCallback))
                content.AppendFormat(@" data-error-callback=""{0}""", errorCallback);

            content.AppendFormat(@">{0}</button>", text);
            content.AppendLine();

            content.AppendFormat(@"<script src=""{0}api.js?hl={1}"" defer></script>", baseUrl, language);

            return content;
        }

        public static IHtmlContent ReCaptchaV3(Uri baseUrl, string siteKey, string action, string language, string callBack, int id, string nonce = null)
        {
            var content = new HtmlContentBuilder();
            content.AppendHtml(@$"<input id=""g-recaptcha-response-{id}"" name=""g-recaptcha-response"" type=""hidden"" value="""" />");
            content.AppendFormat(@"<script src=""{0}api.js?render={1}&hl={2}""", baseUrl, siteKey, language);
            content.AppendNonce(nonce, "></script>");
            content.AppendHtml("");
            content.AppendHtml("<script");
            content.AppendNonce(nonce,">");
            content.AppendHtml($"function updateReCaptcha{id}() {{");
            content.AppendFormat("grecaptcha.execute('{0}', {{action: '{1}'}}).then(function(token){{", siteKey, action);
            content.AppendHtml($"document.getElementById('g-recaptcha-response-{id}').value = token;");
            content.AppendHtml("});");
            content.AppendHtml("}");
            content.AppendHtml($"grecaptcha.ready(function() {{setInterval(updateReCaptcha{id}, 100000); updateReCaptcha{id}()}});");
            content.AppendHtml("</script>");
            content.AppendLine();

            return content;
        }

        private static IHtmlContentBuilder AppendNonce(this IHtmlContentBuilder builder, string nonce, string closingTag)
        {
            if (!string.IsNullOrWhiteSpace(nonce))
                builder.AppendFormat(" nonce=\"{0}\"", nonce);

            builder.AppendHtml(closingTag);
            return builder;
        }
    }
}
