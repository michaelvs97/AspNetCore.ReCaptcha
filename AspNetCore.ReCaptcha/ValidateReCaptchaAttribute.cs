using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace AspNetCore.ReCaptcha
{
    /// <summary>
    /// Validates Recaptcha submitted by a form using: @Html.ReCaptcha()
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    [ExcludeFromCodeCoverage]
    public class ValidateReCaptchaAttribute : Attribute, IFilterFactory
    {
        internal const string DefaultErrorMessage = "Your request cannot be completed because you failed Recaptcha verification.";

        public ValidateReCaptchaAttribute(string action = null)
        {
            Action = action;
        }

        public bool IsReusable => true;

        public string Action { get; set; } = null;

        public string ErrorMessage { get; set; } = null;

        public string FormField { get; set; } = "g-recaptcha-response";

        public IFilterMetadata CreateInstance(IServiceProvider services)
        {
            var recaptchaService = services.GetService<IReCaptchaService>();
            var reCaptchaSettingsSnapshot = services.GetService<IOptionsSnapshot<ReCaptchaSettings>>();
            return new ValidateRecaptchaFilter(recaptchaService, Action, FormField, ErrorMessage, reCaptchaSettingsSnapshot);
        }
    }

    public class ValidateRecaptchaFilter : IAsyncActionFilter, IAsyncPageFilter
    {
        private static ResourceManager _resourceManager;

        private readonly IReCaptchaService _recaptcha;
        private readonly string _action;
        private readonly string _formField;
        private readonly string _modelErrorMessage;
        private readonly ReCaptchaSettings _reCaptchaSettings;

        public ValidateRecaptchaFilter(IReCaptchaService recaptcha, string action, string formField, string modelErrorMessage,
            IOptionsSnapshot<ReCaptchaSettings> reCaptchaSettingsSnapshot)
        {
            _recaptcha = recaptcha;
            _action = action;
            _formField = formField;
            _modelErrorMessage = modelErrorMessage;

            if (reCaptchaSettingsSnapshot is null)
            {
                throw new ArgumentNullException(nameof(reCaptchaSettingsSnapshot));
            }
            
            _reCaptchaSettings = reCaptchaSettingsSnapshot.Value;
        }

        /// <summary>
        /// Gets response from the request form, and tries to validate the response using the ReCaptcha Service.
        /// </summary>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await ValidateRecaptcha(context);
            await next();
        }

        /// <summary>
        /// Gets response from the request form, and tries to validate the response using the ReCaptcha Service.
        /// </summary>
        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (ShouldValidate(context))
                await ValidateRecaptcha(context);

            await next();

            static bool ShouldValidate(ActionContext context)
            {
                return !HttpMethods.IsGet(context.HttpContext.Request.Method)
                    && !HttpMethods.IsHead(context.HttpContext.Request.Method)
                    && !HttpMethods.IsOptions(context.HttpContext.Request.Method);
            }
        }

        [ExcludeFromCodeCoverage]
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }

        private async Task ValidateRecaptcha(ActionContext context)
        {
            if (!_reCaptchaSettings.Enabled)
            {
                // Nothing to do - reCAPTCHA is not enabled / enforced.
                return;
            }

            if (!context.HttpContext.Request.HasFormContentType)
            {
                context.ModelState.AddModelError("", GetErrorMessage(context));
            }
            else
            {
                _ = context.HttpContext.Request.Form.TryGetValue(_formField, out var reCaptchaResponse);
                var isValid = await _recaptcha.VerifyAsync(reCaptchaResponse, _action);
                if (!isValid)
                    context.ModelState.AddModelError("Recaptcha", GetErrorMessage(context));
            }
        }

        private string GetErrorMessage(ActionContext context)
        {
            var localizerFactory = context.HttpContext.RequestServices.GetService<IStringLocalizerFactory>();
            if (localizerFactory != null)
            {
                var settings = context.HttpContext.RequestServices.GetRequiredService<IOptionsSnapshot<ReCaptchaSettings>>();

                IStringLocalizer localizer = null;
                if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    localizer = settings.Value.LocalizerProvider?.Invoke(controllerActionDescriptor.ControllerTypeInfo,
                        localizerFactory);
                }
                else if (context.ActionDescriptor is CompiledPageActionDescriptor pageActionDescriptor)
                {
                    localizer = settings.Value.LocalizerProvider?.Invoke(pageActionDescriptor.HandlerTypeInfo,
                        localizerFactory);
                }

                if (localizer != null)
                {
                    return localizer[_modelErrorMessage ?? ValidateReCaptchaAttribute.DefaultErrorMessage];
                }
            }

            return _modelErrorMessage ?? GetDefaultErrorMessage();
        }

        private static string GetDefaultErrorMessage()
        {
            _resourceManager ??= new ResourceManager("AspNetCore.ReCaptcha.Resources.strings", typeof(ValidateReCaptchaAttribute).Assembly);
            return _resourceManager.GetString(ValidateReCaptchaAttribute.DefaultErrorMessage);
        }
    }
}
