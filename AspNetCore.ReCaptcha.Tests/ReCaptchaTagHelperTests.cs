using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AspNetCore.ReCaptcha.Tests;

public class ReCaptchaTagHelperTests
{
    private readonly ReCaptchaTagHelper _reCaptchaTagHelper;
    private readonly ReCaptchaSettings _reCaptchaSettings;

    public ReCaptchaTagHelperTests()
    {
        // Setup mocks
        _reCaptchaSettings = new ReCaptchaSettings();

        var mockReCaptchaSettingsSnapshot = new Mock<IOptionsSnapshot<ReCaptchaSettings>>();
        mockReCaptchaSettingsSnapshot.Setup(m => m.Value)
            .Returns(_reCaptchaSettings);
        var reCaptchaSettingsSnapshot = mockReCaptchaSettingsSnapshot.Object;
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(reCaptchaSettingsSnapshot);

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

        // Setup SUT
        _reCaptchaTagHelper = new ReCaptchaTagHelper
        {
            ViewContext = viewContext
        };
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void ProcessConsidersAutoThemeSetting(bool autoTheme, bool expectScript)
    {
        // Arrange
        TagHelperContext tagHelperContext = CreateTagHelperContext();
        TagHelperOutput tagHelperOutput = CreateTagHelperOutput();

        _reCaptchaTagHelper.AutoTheme = autoTheme;

        // Act
        _reCaptchaTagHelper.Process(tagHelperContext, tagHelperOutput);

        // Assert
        Assert.True(tagHelperOutput.Content.IsModified);
        string htmlString = tagHelperOutput.Content.GetContent();
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
    
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void ProcessConsidersEnabledSetting(bool enabled, bool expectModified)
    {
        // Arrange
        TagHelperContext tagHelperContext = CreateTagHelperContext();
        TagHelperOutput tagHelperOutput = CreateTagHelperOutput();

        _reCaptchaSettings.Enabled = enabled;

        // Act
        _reCaptchaTagHelper.Process(tagHelperContext, tagHelperOutput);

        // Assert
        Assert.Equal(expectModified, tagHelperOutput.Content.IsModified);
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        const string tagName = "foo-bar";
        var attributes = new TagHelperAttributeList();
        Task<TagHelperContent> GetChildContentAsync(bool useCachedResult, HtmlEncoder encoder) => 
            Task.FromResult<TagHelperContent>(new DefaultTagHelperContent());

        return new TagHelperOutput(tagName, attributes, GetChildContentAsync);
    }

    private static TagHelperContext CreateTagHelperContext()
    {
        var allAttributes = new TagHelperAttributeList();
        var items = new Dictionary<object, object>();
        const string uniqueId = "fizz-buzz";

        return new TagHelperContext(allAttributes, items, uniqueId);
    }
}