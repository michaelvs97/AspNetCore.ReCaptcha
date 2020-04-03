using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.ReCaptcha
{
    /// <summary>
    /// Validates Recaptcha submitted by a form using: @Html.Recaptcha(RecaptchaSettings.Value)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ValidateReCaptchaAttribute : Attribute, IFilterFactory
    {
        private readonly string _modelErrorMessage;

        public bool IsReusable => true;

        public ValidateReCaptchaAttribute(string modelErrorMessage = "Your request cannot be completed because you failed Recaptcha verification.")
        {
            _modelErrorMessage = modelErrorMessage;
        }

        public IFilterMetadata CreateInstance(IServiceProvider services)
        {
            var recaptchaService = services.GetService<IReCaptchaService>();
            return new ValidateRecaptchaFilter(recaptchaService, _modelErrorMessage);
        }
    }

    public class ValidateRecaptchaFilter : IAsyncActionFilter
    {
        private readonly IReCaptchaService _recaptcha;
        private readonly string _modelErrorMessage;

        public ValidateRecaptchaFilter(IReCaptchaService recaptcha, string modelErrorMessage)
        {
            _recaptcha = recaptcha;
            _modelErrorMessage = modelErrorMessage;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.Request.Form.TryGetValue("g-recaptcha-response", out var reCaptchaResponse))
            {
                var valid = await _recaptcha.Verify(reCaptchaResponse);
                if (!valid)
                    context.ModelState.AddModelError("Recaptcha", _modelErrorMessage);
            }

            await next();
        }
    }
}
