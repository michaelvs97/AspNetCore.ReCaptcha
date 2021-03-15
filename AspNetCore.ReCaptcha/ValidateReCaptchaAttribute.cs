using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.ReCaptcha
{
    /// <summary>
    /// Validates Recaptcha submitted by a form using: @Html.ReCaptcha()
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    [ExcludeFromCodeCoverage]
    public class ValidateReCaptchaAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => true;

        public string ErrorMessage { get; set; } = "Your request cannot be completed because you failed Recaptcha verification.";

        public string FormField { get; set; } = "g-recaptcha-response";

        public IFilterMetadata CreateInstance(IServiceProvider services)
        {
            var recaptchaService = services.GetService<IReCaptchaService>();
            return new ValidateRecaptchaFilter(recaptchaService, FormField, ErrorMessage);
        }
    }

    public class ValidateRecaptchaFilter : IAsyncActionFilter, IAsyncPageFilter
    {
        private readonly IReCaptchaService _recaptcha;
        private readonly string _formField;
        private readonly string _modelErrorMessage;

        public ValidateRecaptchaFilter(IReCaptchaService recaptcha, string formField, string modelErrorMessage)
        {
            _recaptcha = recaptcha;
            _formField = formField;
            _modelErrorMessage = modelErrorMessage;
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
            if (!context.HttpContext.Request.HasFormContentType)
            {
                context.ModelState.AddModelError("", _modelErrorMessage);
            }
            else
            {
                _ = context.HttpContext.Request.Form.TryGetValue(_formField, out var reCaptchaResponse);
                var isValid = await _recaptcha.VerifyAsync(reCaptchaResponse);
                if (!isValid)
                    context.ModelState.AddModelError("Recaptcha", _modelErrorMessage);
            }
        }
    }
}
