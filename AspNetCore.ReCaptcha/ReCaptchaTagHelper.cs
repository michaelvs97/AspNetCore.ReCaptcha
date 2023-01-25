using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetCore.ReCaptcha
{
    [HtmlTargetElement("recaptcha")]
    public class ReCaptchaTagHelper : TagHelper
    {
        public string Text { get; set; } = "Submit";
        public string ClassName { get; set; } = "";
        public string Size { get; set; } = "normal";
        public string Theme { get; set; } = "light";
        public string Action { get; set; } = "homepage";
        public string Language { get; set; } = null;
        public string Id { get; set; } = "recaptcha";
        public string Badge { get; set; } = "bottomright";
        public string Callback { get; set; } = null;
        public string ErrorCallback { get; set; } = null;
        public string ExpiredCallback { get; set; } = null;

        /// <summary>
        /// Indicates whether the theme is automatically set to 'dark' based on the user's system settings.
        /// </summary>
        [HtmlAttributeName("auto-theme")]
        public bool AutoTheme { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = null;

            if (string.IsNullOrEmpty(Id))
                throw new ArgumentException("id can't be null");

            if (Id.ToLower() == "submit")
                throw new ArgumentException("id can't be named submit");

            Language ??= ViewContext.HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture.TwoLetterISOLanguageName;

            var settings = ViewContext.HttpContext.RequestServices.GetRequiredService<IOptions<ReCaptchaSettings>>().Value;

            var content = settings.Version switch
            {
                ReCaptchaVersion.V2Invisible => ReCaptchaGenerator.ReCaptchaV2Invisible(settings.RecaptchaBaseUrl, settings.SiteKey, Text, ClassName, Language, Callback, Badge),
                ReCaptchaVersion.V3 => ReCaptchaGenerator.ReCaptchaV3(settings.RecaptchaBaseUrl, settings.SiteKey, Action, Language, Callback, ReCaptchaGenerator.GenerateId(ViewContext)),
                _ => ReCaptchaGenerator.ReCaptchaV2(settings.RecaptchaBaseUrl, settings.SiteKey, Size, Theme, Language, Callback, ErrorCallback, ExpiredCallback, AutoTheme),
            };

            output.Content.AppendHtml(content);
        }
    }
}
