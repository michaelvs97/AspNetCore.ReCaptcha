using System;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetCore.ReCaptcha
{
    public static class ReCaptchaHelper
    {
        public static IServiceCollection AddReCaptcha(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ReCaptchaSettings>(configuration);
            services.AddHttpClient<IReCaptchaService, ReCaptchaService>();
            return services;
        }

        public static IServiceCollection AddReCaptcha(this IServiceCollection services, Action<ReCaptchaSettings> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddHttpClient<IReCaptchaService, ReCaptchaService>();
            return services;
        }

        /// <summary>
        /// Helper extension to render the Google ReCaptcha v2/v3.
        /// </summary>
        /// <param name="helper">Html helper object.</param>
        /// <param name="text">Text shown in on the button. Used in v2-invisible ReCaptcha.</param>
        /// <param name="className">Custom class names added to the generated button of the v2-invisible ReCaptcha.</param>
        /// <param name="size">Optional parameter, contains the size of the widget.</param>
        /// <param name="theme">Google Recaptcha theme default is light.</param>
        /// <param name="action">Google Recaptcha v3 <a href="https://developers.google.com/recaptcha/docs/v3#actions">Action</a></param>
        /// <param name="language">Google Recaptcha <a href="https://developers.google.com/recaptcha/docs/language">Language Code</a></param>
        /// <param name="id">Google ReCaptcha button id. This id can't be named 'submit' due to a naming bug. Used in v2-invisible ReCaptcha.</param>
        /// <param name="badge">Badge parameter for the v2 invisible widget. Defaults to bottomright.</param>
        /// <param name="callback">Google ReCaptcha success callback method. Used in v2 ReCaptcha.</param>
        /// <param name="errorCallback">Google ReCaptcha error callback method. Used in v2 ReCaptcha.</param>
        /// <param name="expiredCallback">Google ReCaptcha expired callback method. Used in v2 ReCaptcha.</param>
        /// <returns>HtmlString with Recaptcha elements</returns>
        public static IHtmlContent ReCaptcha(
            this IHtmlHelper helper,
            string text = "Submit",
            string className = "",
            string size = "normal",
            string theme = "light",
            string action = "homepage",
            string language = "en",
            string id = "recaptcha",
            string badge = "bottomright",
            string callback = null,
            string errorCallback = null,
            string expiredCallback = null)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("id can't be null");

            if (id.ToLower() == "submit")
                throw new ArgumentException("id can't be named submit");

            var uid = Guid.NewGuid();
            var method = uid.ToString().Replace("-", "_");

            var settings = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IOptions<ReCaptchaSettings>>().Value;

            switch (settings.Version)
            {
                default:
                case ReCaptchaVersion.V2:
                    return ReCaptchaV2(settings.SiteKey, size, theme, callback, errorCallback, expiredCallback);
                case ReCaptchaVersion.V2Invisible:
                    return ReCaptchaV2Invisible(settings.SiteKey, text, className, callback, badge);
                case ReCaptchaVersion.V3:
                    return ReCaptchaV3(settings.SiteKey, action, callback);
            }
        }

        private static IHtmlContent ReCaptchaV2(string siteKey, string size, string theme, string callback, string errorCallback, string expiredCallback)
        {
            var content = new HtmlContentBuilder();
            content.AppendFormat(@"<div class=""g-recaptcha"" data-sitekey=""{0}""", siteKey);

            if (!string.IsNullOrEmpty(size))
                content.AppendFormat(@" data-size=""{0}""", size);
            if (!string.IsNullOrEmpty(theme))
                content.AppendFormat(@" data-format=""{0}""", theme);
            if (!string.IsNullOrEmpty(callback))
                content.AppendFormat(@" data-callback=""{0}""", callback);
            if (!string.IsNullOrEmpty(errorCallback))
                content.AppendFormat(@" data-error-callback=""{0}""", errorCallback);
            if (!string.IsNullOrEmpty(expiredCallback))
                content.AppendFormat(@" data-expired-callback=""{0}""", expiredCallback);

            content.AppendFormat("></div>");
            content.AppendLine();
            content.AppendHtmlLine(@"<script src=""https://www.google.com/recaptcha/api.js"" defer></script>");

            return content;
        }

        private static IHtmlContent ReCaptchaV2Invisible(string siteKey, string text, string className, string callback, string badge)
        {
            var content = new HtmlContentBuilder();
            content.AppendFormat(@"<button class=""g-recaptcha {0}""", className);
            content.AppendFormat(@" data-sitekey=""{0}""", siteKey);
            content.AppendFormat(@" data-callback=""{0}""", callback);
            content.AppendFormat(@" data-badge=""{0}""", badge);
            content.AppendFormat(@">{0}</button>", text);
            content.AppendLine();

            content.AppendHtmlLine(@"<script src=""https://www.google.com/recaptcha/api.js"" defer></script>");

            return content;
        }

        private static IHtmlContent ReCaptchaV3(string siteKey, string action, string callBack)
        {
            var content = new HtmlContentBuilder();
            content.AppendHtml(
                @"<input id=""g-recaptcha-response"" name=""g-recaptcha-response"" type=""hidden"" value="""" />");
            content.AppendFormat(@"<script src=""https://www.google.com/recaptcha/api.js?render={0}""></script>", siteKey);
			content.AppendHtml("<script>");
			content.AppendHtml("grecaptcha.ready(function() {");
            content.AppendFormat("grecaptcha.execute('{0}', {{action: '{1}'}}).then(", siteKey, action);
            content.AppendHtml("function(token) {");
            content.AppendHtml("document.getElementById('g-recaptcha-response').value = token;");
            content.AppendHtml("});});");
            content.AppendHtml("</script>");
            content.AppendLine();

            return content;
        }
    }
}
