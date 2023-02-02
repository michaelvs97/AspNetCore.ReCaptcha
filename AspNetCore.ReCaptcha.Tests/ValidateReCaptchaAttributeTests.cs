using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace AspNetCore.ReCaptcha.Tests
{
    public class ValidateReCaptchaAttributeTests
    {
        private readonly IOptionsSnapshot<ReCaptchaSettings> _reCaptchaSettingsSnapshot;
        private readonly ReCaptchaSettings _reCaptchaSettings;

        public ValidateReCaptchaAttributeTests()
        {
            _reCaptchaSettings = new ReCaptchaSettings();

            var mockReCaptchaSettingsSnapshot = new Mock<IOptionsSnapshot<ReCaptchaSettings>>();
            mockReCaptchaSettingsSnapshot.Setup(m => m.Value)
                .Returns(_reCaptchaSettings);
            _reCaptchaSettingsSnapshot = mockReCaptchaSettingsSnapshot.Object;
        }
        
        public class OnActionExecutionAsync : ValidateReCaptchaAttributeTests
        {
            private static ActionExecutingContext CreateActionExecutingContext(Mock<HttpContext> httpContextMock, ActionContext actionContext, StringValues expected)
            {
                httpContextMock.Setup(x => x.Request.HasFormContentType).Returns(true);
                httpContextMock.Setup(x => x.Request.Form.TryGetValue(It.IsAny<string>(), out expected)).Returns(true);

                return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(),
                    new Dictionary<string, object>(), Mock.Of<Controller>());
            }

            private static ActionContext CreateActionContext(IMock<HttpContext> httpContextMock, ModelStateDictionary modelState, ActionDescriptor actionDescriptor)
            {
                return new(httpContextMock.Object,
                    Mock.Of<RouteData>(),
                    actionDescriptor,
                    modelState);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public async Task VerifyAsyncReturnsBoolean(bool success)
            {
                var reCaptchaServiceMock = new Mock<IReCaptchaService>();

                reCaptchaServiceMock.Setup(x => x.VerifyAsync(It.IsAny<string>(), null)).Returns(Task.FromResult(success));

                var filter = new ValidateRecaptchaFilter(reCaptchaServiceMock.Object, null, "", "", _reCaptchaSettingsSnapshot);

                var expected = new StringValues("123");

                var serviceProviderMock = new Mock<IServiceProvider>();

                var httpContextMock = new Mock<HttpContext>();
                httpContextMock.Setup(x => x.RequestServices)
                    .Returns(serviceProviderMock.Object);

                var modelState = new ModelStateDictionary();

                var actionDescriptor = new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(ValidateReCaptchaAttributeTests).GetTypeInfo(),
                };

                var actionContext = CreateActionContext(httpContextMock, modelState, actionDescriptor);

                var actionExecutingContext = CreateActionExecutingContext(httpContextMock, actionContext, expected);

                Task<ActionExecutedContext> Next()
                {
                    var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
                    return Task.FromResult(ctx);
                }

                await filter.OnActionExecutionAsync(actionExecutingContext, Next);
                reCaptchaServiceMock.Verify(x => x.VerifyAsync(It.IsAny<string>(), null), Times.Once);
                if(!success)
                    Assert.Equal(1, modelState.ErrorCount);
            }

            [Theory]
            [InlineData(true, false)]
            [InlineData(false, true)]
            public async Task VerifyAsyncConsidersEnabledFlag(bool enabled, bool expectSuccess)
            {
                // Arrange
                _reCaptchaSettings.Enabled = enabled;
                
                var reCaptchaServiceMock = new Mock<IReCaptchaService>();

                reCaptchaServiceMock.Setup(x => 
                    x.VerifyAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(false);

                var filter = new ValidateRecaptchaFilter(reCaptchaServiceMock.Object, null, "", "", 
                    _reCaptchaSettingsSnapshot);

                var expected = new StringValues("123");

                var serviceProviderMock = new Mock<IServiceProvider>();

                var httpContextMock = new Mock<HttpContext>();
                httpContextMock.Setup(x => x.RequestServices)
                    .Returns(serviceProviderMock.Object);

                var modelState = new ModelStateDictionary();

                var actionDescriptor = new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(ValidateReCaptchaAttributeTests).GetTypeInfo(),
                };

                var actionContext = CreateActionContext(httpContextMock, modelState, actionDescriptor);

                var actionExecutingContext = CreateActionExecutingContext(httpContextMock, actionContext, expected);

                Task<ActionExecutedContext> Next()
                {
                    var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
                    return Task.FromResult(ctx);
                }

                // Act
                await filter.OnActionExecutionAsync(actionExecutingContext, Next);
                
                // Assert
                Assert.Equal(expectSuccess ? 0 : 1, modelState.ErrorCount);
            }

            [Fact]
            public async Task VerifyAsyncLocalizesErrorMessage()
            {
                var reCaptchaServiceMock = new Mock<IReCaptchaService>();

                reCaptchaServiceMock.Setup(x => x.VerifyAsync(It.IsAny<string>(), null)).Returns(Task.FromResult(false));

                var filter = new ValidateRecaptchaFilter(reCaptchaServiceMock.Object, null, "", null, _reCaptchaSettingsSnapshot);

                var expected = new StringValues("123");

                var stringLocalizerMock = new Mock<IStringLocalizer>();
                stringLocalizerMock.Setup(x => x[ValidateReCaptchaAttribute.DefaultErrorMessage])
                    .Returns(new LocalizedString("", "Localized error message"));

                var stringLocalizerFactory = new Mock<IStringLocalizerFactory>();
                stringLocalizerFactory.Setup(x => x.Create(It.IsAny<Type>()))
                    .Returns(stringLocalizerMock.Object);

                var serviceProviderMock = new Mock<IServiceProvider>();
                serviceProviderMock.Setup(x => x.GetService(typeof(IStringLocalizerFactory)))
                    .Returns(stringLocalizerFactory.Object);

                serviceProviderMock.Setup(x => x.GetService(typeof(IOptions<ReCaptchaSettings>)))
                    .Returns(new OptionsWrapper<ReCaptchaSettings>(new ReCaptchaSettings { LocalizerProvider = (type, factory) => factory.Create(type) }));

                var httpContextMock = new Mock<HttpContext>();
                httpContextMock.Setup(x => x.RequestServices)
                    .Returns(serviceProviderMock.Object);

                var modelState = new ModelStateDictionary();

                var actionDescriptor = new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(ValidateReCaptchaAttributeTests).GetTypeInfo(),
                };

                var actionContext = CreateActionContext(httpContextMock, modelState, actionDescriptor);

                var actionExecutingContext = CreateActionExecutingContext(httpContextMock, actionContext, expected);

                Task<ActionExecutedContext> Next()
                {
                    var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
                    return Task.FromResult(ctx);
                }

                await filter.OnActionExecutionAsync(actionExecutingContext, Next);
                reCaptchaServiceMock.Verify(x => x.VerifyAsync(It.IsAny<string>(), null), Times.Once);

                Assert.Equal(1, modelState.ErrorCount);
                var errorMessage = modelState.First(x => x.Key == "Recaptcha").Value.Errors.Single().ErrorMessage;
                Assert.Equal("Localized error message", errorMessage);
            }

            [Fact]
            public async Task VerifyAsyncWithAction()
            {
                var reCaptchaServiceMock = new Mock<IReCaptchaService>();

                reCaptchaServiceMock.Setup(x => x.VerifyAsync(It.IsAny<string>(), "action")).ReturnsAsync(true);

                var filter = new ValidateRecaptchaFilter(reCaptchaServiceMock.Object, "action", "", "", _reCaptchaSettingsSnapshot);

                var expected = new StringValues("123");

                var serviceProviderMock = new Mock<IServiceProvider>();

                var httpContextMock = new Mock<HttpContext>();
                httpContextMock.Setup(x => x.RequestServices)
                    .Returns(serviceProviderMock.Object);

                var modelState = new ModelStateDictionary();

                var actionDescriptor = new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(ValidateReCaptchaAttributeTests).GetTypeInfo(),
                };

                var actionContext = CreateActionContext(httpContextMock, modelState, actionDescriptor);

                var actionExecutingContext = CreateActionExecutingContext(httpContextMock, actionContext, expected);

                Task<ActionExecutedContext> Next()
                {
                    var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
                    return Task.FromResult(ctx);
                }

                await filter.OnActionExecutionAsync(actionExecutingContext, Next);
                reCaptchaServiceMock.Verify(x => x.VerifyAsync(It.IsAny<string>(), "action"), Times.Once);
                Assert.Equal(0, modelState.ErrorCount);
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

                reCaptchaServiceMock.Setup(x => x.VerifyAsync(It.IsAny<string>(), null)).Returns(Task.FromResult(success));

                var filter = new ValidateRecaptchaFilter(reCaptchaServiceMock.Object, null, "", "", _reCaptchaSettingsSnapshot);

                var expected = new StringValues("123");

                var serviceProviderMock = new Mock<IServiceProvider>();

                var httpContextMock = new Mock<HttpContext>();
                httpContextMock.Setup(x => x.RequestServices)
                    .Returns(serviceProviderMock.Object);

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
                reCaptchaServiceMock.Verify(x => x.VerifyAsync(It.IsAny<string>(), null), Times.Once);
            }

            [Theory]
            [InlineData(true, false)]
            [InlineData(false, true)]
            public async Task VerifyAsyncConsidersEnabledFlag(bool enabled, bool expectSuccess)
            {
                // Arrange
                _reCaptchaSettings.Enabled = enabled;
                
                var reCaptchaServiceMock = new Mock<IReCaptchaService>();

                reCaptchaServiceMock.Setup(x => 
                    x.VerifyAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(false);

                var filter = new ValidateRecaptchaFilter(reCaptchaServiceMock.Object, null, "", "", _reCaptchaSettingsSnapshot);

                var expected = new StringValues("123");

                var serviceProviderMock = new Mock<IServiceProvider>();

                var httpContextMock = new Mock<HttpContext>();
                httpContextMock.Setup(x => x.RequestServices)
                    .Returns(serviceProviderMock.Object);

                var pageContext = CreatePageContext(new ActionContext(httpContextMock.Object, new RouteData(), new ActionDescriptor()));

                var model = new Mock<PageModel>();

                var pageHandlerExecutedContext = new PageHandlerExecutedContext(
                    pageContext,
                    Array.Empty<IFilterMetadata>(),
                    new HandlerMethodDescriptor(),
                    model.Object);

                var actionExecutingContext = CreatePageHandlerExecutingContext(httpContextMock, pageContext, expected, model);

                Task<PageHandlerExecutedContext> Next() => Task.FromResult(pageHandlerExecutedContext);

                // Act
                await filter.OnPageHandlerExecutionAsync(actionExecutingContext, Next);
                
                // Assert
                ModelStateDictionary modelState = actionExecutingContext.ModelState;
                Assert.Equal(expectSuccess ? 0 : 1, modelState.ErrorCount);
            }

            [Fact]
            public async Task VerifyAsyncLocalizesErrorMessage()
            {
                var reCaptchaServiceMock = new Mock<IReCaptchaService>();

                reCaptchaServiceMock.Setup(x => x.VerifyAsync(It.IsAny<string>(), null)).Returns(Task.FromResult(false));

                var filter = new ValidateRecaptchaFilter(reCaptchaServiceMock.Object, null, "", "Custom Error Message", _reCaptchaSettingsSnapshot);

                var expected = new StringValues("123");

                var stringLocalizerMock = new Mock<IStringLocalizer>();
                stringLocalizerMock.Setup(x => x["Custom Error Message"])
                    .Returns(new LocalizedString("", "Localized error message"));

                var stringLocalizerFactory = new Mock<IStringLocalizerFactory>();
                stringLocalizerFactory.Setup(x => x.Create(It.IsAny<Type>()))
                    .Returns(stringLocalizerMock.Object);

                var serviceProviderMock = new Mock<IServiceProvider>();
                serviceProviderMock.Setup(x => x.GetService(typeof(IStringLocalizerFactory)))
                    .Returns(stringLocalizerFactory.Object);

                serviceProviderMock.Setup(x => x.GetService(typeof(IOptions<ReCaptchaSettings>)))
                    .Returns(new OptionsWrapper<ReCaptchaSettings>(new ReCaptchaSettings { LocalizerProvider = (type, factory) => factory.Create(type) }));

                var httpContextMock = new Mock<HttpContext>();
                httpContextMock.Setup(x => x.RequestServices)
                    .Returns(serviceProviderMock.Object);

                var modelState = new ModelStateDictionary();

                var actionDescriptor = new CompiledPageActionDescriptor
                {
                    HandlerTypeInfo = typeof(ValidateReCaptchaAttributeTests).GetTypeInfo(),
                };

                var pageContext = CreatePageContext(new ActionContext(httpContextMock.Object, new RouteData(), actionDescriptor, modelState));

                var model = new Mock<PageModel>();

                var pageHandlerExecutedContext = new PageHandlerExecutedContext(
                    pageContext,
                    Array.Empty<IFilterMetadata>(),
                    new HandlerMethodDescriptor(),
                    model.Object);

                var actionExecutingContext = CreatePageHandlerExecutingContext(httpContextMock, pageContext, expected, model);

                PageHandlerExecutionDelegate next = () => Task.FromResult(pageHandlerExecutedContext);

                await filter.OnPageHandlerExecutionAsync(actionExecutingContext, next);
                reCaptchaServiceMock.Verify(x => x.VerifyAsync(It.IsAny<string>(), null), Times.Once);

                Assert.Equal(1, modelState.ErrorCount);
                var errorMessage = modelState.First(x => x.Key == "Recaptcha").Value.Errors.Single().ErrorMessage;
                Assert.Equal("Localized error message", errorMessage);
            }

            [Fact]
            public async Task VerifyAsyncUsesAction()
            {
                var reCaptchaServiceMock = new Mock<IReCaptchaService>();

                reCaptchaServiceMock.Setup(x => x.VerifyAsync(It.IsAny<string>(), "action")).Returns(Task.FromResult(true));

                var filter = new ValidateRecaptchaFilter(reCaptchaServiceMock.Object, "action", "", "Custom Error Message", _reCaptchaSettingsSnapshot);

                var expected = new StringValues("123");

                var stringLocalizerMock = new Mock<IStringLocalizer>();
                stringLocalizerMock.Setup(x => x["Custom Error Message"])
                    .Returns(new LocalizedString("", "Localized error message"));

                var stringLocalizerFactory = new Mock<IStringLocalizerFactory>();
                stringLocalizerFactory.Setup(x => x.Create(It.IsAny<Type>()))
                    .Returns(stringLocalizerMock.Object);

                var serviceProviderMock = new Mock<IServiceProvider>();
                serviceProviderMock.Setup(x => x.GetService(typeof(IStringLocalizerFactory)))
                    .Returns(stringLocalizerFactory.Object);

                serviceProviderMock.Setup(x => x.GetService(typeof(IOptions<ReCaptchaSettings>)))
                    .Returns(new OptionsWrapper<ReCaptchaSettings>(new ReCaptchaSettings { LocalizerProvider = (type, factory) => factory.Create(type) }));

                var httpContextMock = new Mock<HttpContext>();
                httpContextMock.Setup(x => x.RequestServices)
                    .Returns(serviceProviderMock.Object);

                var modelState = new ModelStateDictionary();

                var actionDescriptor = new CompiledPageActionDescriptor
                {
                    HandlerTypeInfo = typeof(ValidateReCaptchaAttributeTests).GetTypeInfo(),
                };

                var pageContext = CreatePageContext(new ActionContext(httpContextMock.Object, new RouteData(), actionDescriptor, modelState));

                var model = new Mock<PageModel>();

                var pageHandlerExecutedContext = new PageHandlerExecutedContext(
                    pageContext,
                    Array.Empty<IFilterMetadata>(),
                    new HandlerMethodDescriptor(),
                    model.Object);

                var actionExecutingContext = CreatePageHandlerExecutingContext(httpContextMock, pageContext, expected, model);

                PageHandlerExecutionDelegate next = () => Task.FromResult(pageHandlerExecutedContext);

                await filter.OnPageHandlerExecutionAsync(actionExecutingContext, next);
                reCaptchaServiceMock.Verify(x => x.VerifyAsync(It.IsAny<string>(), "action"), Times.Once);

                Assert.Equal(0, modelState.ErrorCount);
            }
        }
    }
}
