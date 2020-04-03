using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.ReCaptcha
{
    /// <summary>
    /// Validates Recaptcha submitted by a form using: @Html.ReCaptcha()
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
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

    public class ValidateRecaptchaFilter : IAsyncActionFilter
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

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.Request.Form.TryGetValue(_formField, out var reCaptchaResponse))
            {
                var isValid = await _recaptcha.Verify(reCaptchaResponse);
                if (!isValid)
                    context.ModelState.AddModelError("Recaptcha", _modelErrorMessage);
            }

            await next();
        }
    }
}
