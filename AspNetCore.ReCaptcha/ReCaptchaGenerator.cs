using System;
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

        public static IHtmlContent ReCaptchaV2(Uri baseUrl, string siteKey, string size, string theme, string language, string callback, string errorCallback, string expiredCallback)
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
            content.AppendFormat(@"<script src=""{0}api.js?hl={1}"" defer></script>", baseUrl, language);

            return content;
        }

        public static IHtmlContent ReCaptchaV2Invisible(Uri baseUrl, string siteKey, string text, string className, string language, string callback, string badge)
        {
            var content = new HtmlContentBuilder();
            content.AppendFormat(@"<button class=""g-recaptcha {0}""", className);
            content.AppendFormat(@" data-sitekey=""{0}""", siteKey);
            content.AppendFormat(@" data-callback=""{0}""", callback);
            content.AppendFormat(@" data-badge=""{0}""", badge);
            content.AppendFormat(@">{0}</button>", text);
            content.AppendLine();

            content.AppendFormat(@"<script src=""{0}api.js?hl={1}"" defer></script>", baseUrl, language);

            return content;
        }

        public static IHtmlContent ReCaptchaV3(Uri baseUrl, string siteKey, string action, string language, string callBack, int id)
        {
            var content = new HtmlContentBuilder();
            content.AppendHtml(@$"<input id=""g-recaptcha-response-{id}"" name=""g-recaptcha-response"" type=""hidden"" value="""" />");
            content.AppendFormat(@"<script src=""{0}api.js?render={1}&hl={2}""></script>", baseUrl, siteKey, language);
            content.AppendHtml("<script>");
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
    }
}
