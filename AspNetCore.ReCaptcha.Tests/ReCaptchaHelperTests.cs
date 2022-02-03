using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace AspNetCore.ReCaptcha.Tests;

public class ReCaptchaHelperTests
{
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
        Assert.Equal("https://www.recaptcha.net/", settings.RecaptchaBaseUrl.ToString());
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
}
