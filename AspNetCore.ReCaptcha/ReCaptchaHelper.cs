using System;
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
        /// <param name="version">Specifies which version of ReCaptcha to use.</param>
        /// <param name="size">Optional parameter, contains the size of the widget.</param>
        /// <param name="theme">Google Recaptcha theme default is light.</param>
        /// <param name="action">Google Recaptcha v3 <a href="https://developers.google.com/recaptcha/docs/v3#actions">Action</a></param>
        /// <param name="language">Google Recaptcha <a href="https://developers.google.com/recaptcha/docs/language">Language Code</a></param>
        /// <param name="id">Google ReCaptcha button id. This id can't be named 'submit' due to a naming bug. Used in v2-invis ReCaptcha.</param>
        /// <param name="successCallback">Google ReCaptcha success callback method. Used in v2 ReCaptcha.</param>
        /// <param name="errorCallback">Google ReCaptcha error callback method. Used in v2 ReCaptcha.</param>
        /// <param name="expiredCallback">Google ReCaptcha expired callback method. Used in v2 ReCaptcha.</param>
        /// <returns>HtmlString with Recaptcha elements</returns>
        public static IHtmlContent ReCaptcha(this IHtmlHelper helper, ReCaptchaVersion? version = null, string size = "normal", string theme = "light", string action = "homepage", string language = "en", string id = "recaptcha", string successCallback = null, string errorCallback = null, string expiredCallback = null)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("id can't be null");

            if (id.ToLower() == "submit")
                throw new ArgumentException("id can't be named submit");

            var uid = Guid.NewGuid();
            var method = uid.ToString().Replace("-", "_");

            var settings = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IOptions<ReCaptchaSettings>>().Value;

            switch (version)
            {
                default:
                case ReCaptchaVersion.V2:
                    return ReCaptchaV2(settings.SiteKey, size, theme, successCallback, errorCallback, expiredCallback);
                case ReCaptchaVersion.V2Invisible:
                    return ReCaptchaV2Invisible();
                case ReCaptchaVersion.V3:
                    return ReCaptchaV3();
            }
        }

        /// <summary>
        /// HtmlContentBuilder for v2 ReCaptcha.
        /// </summary>
        /// <param name="siteKey">Google ReCaptcha site key.</param>
        /// <param name="size">Size of the widget.</param>
        /// <param name="theme">Google ReCaptcha theme. Default is light.</param>
        /// <param name="successCallback">Google ReCaptcha success callback method. Used in v2 ReCaptcha.</param>
        /// <param name="errorCallback">Google ReCaptcha error callback method. Used in v2 ReCaptcha.</param>
        /// <param name="expiredCallback">Google ReCaptcha expired callback method. Used in v2 ReCaptcha.</param>
        /// <returns>HTMLContent of ReCaptcha element.</returns>
        private static IHtmlContent ReCaptchaV2(string siteKey, string size, string theme, string successCallback, string errorCallback, string expiredCallback)
        {
            var id = Guid.NewGuid().ToString().Replace("-", "");

            var content = new HtmlContentBuilder();
            content.AppendFormat(@"<div class=""g-recaptcha"" data-sitekey=""{0}""", siteKey);

            if (!string.IsNullOrEmpty(size))
                content.AppendFormat(@" data-size=""{0}""", size);
            if (!string.IsNullOrEmpty(theme))
                content.AppendFormat(@" data-format=""{0}""", theme);
            if (!string.IsNullOrEmpty(successCallback))
                content.AppendFormat(@" data-callback=""{0}""", successCallback);
            if (!string.IsNullOrEmpty(errorCallback))
                content.AppendFormat(@" data-error-callback=""{0}""", errorCallback);
            if (!string.IsNullOrEmpty(expiredCallback))
                content.AppendFormat(@" data-expired-callback=""{0}""", expiredCallback);

            content.AppendFormat("></div>");
            content.AppendLine();
            content.AppendHtmlLine(@"<script src=""https://www.google.com/recaptcha/api.js"" defer></script>");

            return content;
        }

        private static IHtmlContent ReCaptchaV2Invisible()
        {
            throw new NotImplementedException();
        }

        private static IHtmlContent ReCaptchaV3()
        {
            throw new NotImplementedException();
        }
    }
}
