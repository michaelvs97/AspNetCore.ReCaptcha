using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace AspNetCore.ReCaptcha.Tests
{
    public class ValidateReCaptchaAttributeTests
    {
        public class OnActionExecutionAsync : ValidateReCaptchaAttributeTests
        {
            private static ActionExecutingContext CreateActionExecutingContext(Mock<HttpContext> httpContextMock, ActionContext actionContext, StringValues expected)
            {
                httpContextMock.Setup(x => x.Request.HasFormContentType).Returns(true);
                httpContextMock.Setup(x => x.Request.Form.TryGetValue(It.IsAny<string>(), out expected)).Returns(true);
                
                return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(),
                    new Dictionary<string, object>(), Mock.Of<Controller>());
            }

            private static ActionContext CreateActionContext(IMock<HttpContext> httpContextMock, ModelStateDictionary modelState)
            {
                return new(httpContextMock.Object,
                    Mock.Of<RouteData>(),
                    Mock.Of<ActionDescriptor>(),
                    modelState);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public async Task VerifyAsyncReturnsBoolean(bool success)
            {
                var reCaptchaServiceMock = new Mock<IReCaptchaService>();

                reCaptchaServiceMock.Setup(x => x.VerifyAsync(It.IsAny<string>())).Returns(Task.FromResult(success));

                var filter = new ValidateRecaptchaFilter(reCaptchaServiceMock.Object, "", "");

                var expected = new StringValues("123");

                var httpContextMock = new Mock<HttpContext>();
                
                var modelState = new ModelStateDictionary();

                var actionContext = CreateActionContext(httpContextMock, modelState);

                var actionExecutingContext = CreateActionExecutingContext(httpContextMock, actionContext, expected);

                Task<ActionExecutedContext> Next()
                {
                    var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
                    return Task.FromResult(ctx);
                }

                await filter.OnActionExecutionAsync(actionExecutingContext, Next);
                reCaptchaServiceMock.Verify(x => x.VerifyAsync(It.IsAny<string>()), Times.Once);
                if(!success)
                    Assert.Equal(1, modelState.ErrorCount);
            }
        }

        public class OnPageHandlerExecutionAsync : ValidateReCaptchaAttributeTests
        {
            private static PageHandlerExecutingContext CreatePageHandlerExecutingContext(Mock<HttpContext> httpContextMock, PageContext pageContext, StringValues expected, Mock<PageModel> pageModelMock)
            {
                httpContextMock.Setup(x => x.Request.HasFormContentType).Returns(true);
                httpContextMock.Setup(x => x.Request.Form.TryGetValue(It.IsAny<string>(), out expected)).Returns(true);

                return new PageHandlerExecutingContext(pageContext, new List<IFilterMetadata>(), new HandlerMethodDescriptor(), new Dictionary<string, object>(), pageModelMock.Object);
            }

            private static PageContext CreatePageContext(ActionContext actionContext)
            {
                return new PageContext(actionContext);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public async Task VerifyAsyncReturnsBoolean(bool success)
            {
                var reCaptchaServiceMock = new Mock<IReCaptchaService>();

                reCaptchaServiceMock.Setup(x => x.VerifyAsync(It.IsAny<string>())).Returns(Task.FromResult(success));

                var filter = new ValidateRecaptchaFilter(reCaptchaServiceMock.Object, "", "");

                var expected = new StringValues("123");

                var httpContextMock = new Mock<HttpContext>();
                
                var pageContext = CreatePageContext(new ActionContext(httpContextMock.Object, new RouteData(), new ActionDescriptor()));

                var model = new Mock<PageModel>();

                var pageHandlerExecutedContext = new PageHandlerExecutedContext(
                    pageContext,
                    Array.Empty<IFilterMetadata>(),
                    new HandlerMethodDescriptor(),
                    model.Object);

                var actionExecutingContext = CreatePageHandlerExecutingContext(httpContextMock, pageContext, expected, model);

                PageHandlerExecutionDelegate next = () => Task.FromResult(pageHandlerExecutedContext);

                await filter.OnPageHandlerExecutionAsync(actionExecutingContext, next);
                reCaptchaServiceMock.Verify(x => x.VerifyAsync(It.IsAny<string>()), Times.Once);
            }
        }
    }
}
