using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetCore.ReCaptcha
{
    [ExcludeFromCodeCoverage]
    public static class ReCaptchaHelper
    {
        private static IHttpClientBuilder AddReCaptchaServices(this IServiceCollection services)
        {
            services.AddLogging();

            services.PostConfigure<ReCaptchaSettings>(settings =>
            {
                settings.LocalizerProvider ??= (modelType, localizerFactory) => localizerFactory.Create(modelType);

                if (settings.UseRecaptchaNet)
                    settings.RecaptchaBaseUrl = ReCaptchaSettings.RecaptchaNetBaseUrl;

                if (!settings.RecaptchaBaseUrl.IsAbsoluteUri)
                    throw new Exception("Invalid ReCaptcha settings, RecaptchaBaseUrl must be an absolute URI.");

                if (settings.RecaptchaBaseUrl.Scheme != "https")
                    throw new Exception("Invalid ReCaptcha settings, RecaptchaBaseUrl must use HTTPS.");

                if (!settings.RecaptchaBaseUrl.AbsolutePath.EndsWith('/'))
                    settings.RecaptchaBaseUrl = new Uri(settings.RecaptchaBaseUrl, $"{settings.RecaptchaBaseUrl.AbsolutePath}/");
            });

            var httpBuilder = services.AddHttpClient<IReCaptchaService, ReCaptchaService>((sp, client) =>
            {
                client.BaseAddress = sp.GetRequiredService<IOptions<ReCaptchaSettings>>().Value.RecaptchaBaseUrl;
            });

            return httpBuilder;
        }

        public static IServiceCollection AddReCaptcha(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ReCaptchaSettings>(configuration);
            services.AddReCaptchaServices();
            return services;
        }

        public static IServiceCollection AddReCaptcha(this IServiceCollection services, Action<ReCaptchaSettings> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddReCaptchaServices();
            return services;
        }

        public static IServiceCollection AddReCaptcha(this IServiceCollection services, Action<ReCaptchaSettings> configureOptions, Action<IHttpClientBuilder> configureHttpClient)
        {
            services.Configure(configureOptions);
            var httpClientBuilder = services.AddReCaptchaServices();
            configureHttpClient(httpClientBuilder);
            return services;
        }

        public static IServiceCollection AddReCaptcha(this IServiceCollection services, IConfiguration configuration, Action<ReCaptchaSettings> configureOptions)
        {
            services.Configure<ReCaptchaSettings>(configuration);
            services.Configure(configureOptions);
            services.AddReCaptchaServices();
            return services;
        }

        public static IServiceCollection AddReCaptcha(this IServiceCollection services, IConfiguration configuration, Action<ReCaptchaSettings> configureOptions, Action<IHttpClientBuilder> configureHttpClient)
        {
            services.Configure<ReCaptchaSettings>(configuration);
            services.Configure(configureOptions);
            var httpClientBuilder = services.AddReCaptchaServices();
            configureHttpClient(httpClientBuilder);
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
            string language = null,
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

            language ??= helper.ViewContext.HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture
                .UICulture.TwoLetterISOLanguageName;

            var settings = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IOptions<ReCaptchaSettings>>().Value;

            switch (settings.Version)
            {
                default:
                case ReCaptchaVersion.V2:
                    return ReCaptchaGenerator.ReCaptchaV2(settings.RecaptchaBaseUrl, settings.SiteKey, size, theme, language, callback, errorCallback, expiredCallback);
                case ReCaptchaVersion.V2Invisible:
                    return ReCaptchaGenerator.ReCaptchaV2Invisible(settings.RecaptchaBaseUrl, settings.SiteKey, text, className, language, callback, badge);
                case ReCaptchaVersion.V3:
                    return ReCaptchaGenerator.ReCaptchaV3(settings.RecaptchaBaseUrl, settings.SiteKey, action, language, callback, ReCaptchaGenerator.GenerateId(helper.ViewContext));
            }
        }
    }
}
