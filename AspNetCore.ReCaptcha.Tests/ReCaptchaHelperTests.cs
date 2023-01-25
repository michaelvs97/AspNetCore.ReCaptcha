using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AspNetCore.ReCaptcha.Tests;

public class ReCaptchaHelperTests
{
    private readonly IHtmlHelper _htmlHelper;

    public ReCaptchaHelperTests()
    {
        // Setup mocks
        var reCaptchaSettings = new ReCaptchaSettings();

        var mockReCaptchaSettingsSnapshot = new Mock<IOptionsSnapshot<ReCaptchaSettings>>();
        mockReCaptchaSettingsSnapshot.Setup(m => m.Value)
            .Returns(reCaptchaSettings);
        var reCaptchaSettingsSnapshot = mockReCaptchaSettingsSnapshot.Object;
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IOptions<ReCaptchaSettings>>(reCaptchaSettingsSnapshot);

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        var featureCollection = new FeatureCollection();

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.RequestServices)
            .Returns(serviceProvider);
        mockHttpContext.Setup(m => m.Features)
            .Returns(featureCollection);
        HttpContext httpContext = mockHttpContext.Object;

        var viewContext = new ViewContext
        {
            HttpContext = httpContext
        };

        var mockHtmlHelper = new Mock<IHtmlHelper>();
        mockHtmlHelper.Setup(m => m.ViewContext)
            .Returns(viewContext);
        _htmlHelper = mockHtmlHelper.Object;
    }
    
    [Fact]
    public void AddReCaptcha_DefaultValues()
    {
        var services = new ServiceCollection();
        services.AddReCaptcha(_ => { });

        var settings = services.BuildServiceProvider().GetRequiredService<IOptions<ReCaptchaSettings>>().Value;

        Assert.Equal(ReCaptchaVersion.V2, settings.Version);
        Assert.False(settings.UseRecaptchaNet);
        Assert.Equal("https://www.google.com/recaptcha/", settings.RecaptchaBaseUrl.ToString());
    }

    [Fact]
    public void AddReCaptcha_UseRecaptchaNet()
    {
        var services = new ServiceCollection();
        services.AddReCaptcha(opt => opt.UseRecaptchaNet = true);

        var settings = services.BuildServiceProvider().GetRequiredService<IOptions<ReCaptchaSettings>>().Value;

        Assert.Equal(ReCaptchaVersion.V2, settings.Version);
        Assert.True(settings.UseRecaptchaNet);
        Assert.Equal("https://www.recaptcha.net/recaptcha/", settings.RecaptchaBaseUrl.ToString());
    }

    [Fact]
    public void AddReCaptcha_CustomBaseUri()
    {
        var services = new ServiceCollection();
        services.AddReCaptcha(opt => opt.RecaptchaBaseUrl = new Uri("https://myhost.com/recaptcha"));

        var settings = services.BuildServiceProvider().GetRequiredService<IOptions<ReCaptchaSettings>>().Value;

        Assert.Equal(ReCaptchaVersion.V2, settings.Version);
        Assert.False(settings.UseRecaptchaNet);
        Assert.Equal("https://myhost.com/recaptcha/", settings.RecaptchaBaseUrl.ToString());
    }

    [Theory]
    [InlineData("relative/url", "Invalid ReCaptcha settings, RecaptchaBaseUrl must be an absolute URI.")]
    [InlineData("http://host/", "Invalid ReCaptcha settings, RecaptchaBaseUrl must use HTTPS.")]
    [InlineData("file://wrong", "Invalid ReCaptcha settings, RecaptchaBaseUrl must use HTTPS.")]
    public void AddReCaptcha_InvalidBaseUri(string url, string message)
    {
        var services = new ServiceCollection();
        services.AddReCaptcha(opt => opt.RecaptchaBaseUrl = new Uri(url, UriKind.RelativeOrAbsolute));

        var ex = Assert.Throws<Exception>(() =>
        {
            _ = services.BuildServiceProvider().GetRequiredService<IOptions<ReCaptchaSettings>>().Value;
        });

        Assert.Equal(message, ex.Message);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void ReCaptchaConsidersAutoThemeArgument(bool autoTheme, bool expectScript)
    {
        // Arrange
        const string text = "foo";
        const string className = "foo";
        const string size = "foo";
        const string theme = "foo";
        const string action = "foo";
        const string language = "foo";
        const string id = "foo";
        const string badge = "foo";
        const string callback = "foo";
        const string errorCallback = "foo";
        const string expiredCallback = "foo";
        
        // Act
        IHtmlContent htmlContent = _htmlHelper.ReCaptcha(text, className, size, theme, action, language, id, badge, callback,
            errorCallback, expiredCallback, autoTheme);

        // Assert
        Assert.NotNull(htmlContent);
        string htmlString = htmlContent.ToHtmlString();
        const string mediaQueryString = "prefers-color-scheme";
        if (expectScript)
        {
            Assert.Contains(mediaQueryString, htmlString);
        }
        else
        {
            Assert.DoesNotContain(mediaQueryString, htmlString);
        }
    }
}
