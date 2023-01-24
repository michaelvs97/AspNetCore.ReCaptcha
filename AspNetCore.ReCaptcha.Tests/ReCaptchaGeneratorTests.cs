using System;
using Microsoft.AspNetCore.Html;
using Xunit;

namespace AspNetCore.ReCaptcha.Tests
{
    public class ReCaptchaGeneratorTests
    {
        [Fact]
        public void ReCaptchaGeneratorReturnsReCaptchaForV2()
        {
            var result = ReCaptchaGenerator.ReCaptchaV2(new Uri("https://www.google.com/recaptcha/"), "test", "test", "test", "test", "test", "test", "test");

            Assert.NotNull(result);
        }

        [Fact]
        public void ReCaptchaGeneratorReturnsReCaptchaForV2Invisible()
        {
            var result = ReCaptchaGenerator.ReCaptchaV2Invisible(new Uri("https://www.google.com/recaptcha/"), "test", "test", "test", "test", "test", "test");

            Assert.NotNull(result);
        }

        [Fact]
        public void ReCaptchaGeneratorReturnsReCaptchaForV3()
        {
            var result = ReCaptchaGenerator.ReCaptchaV3(new Uri("https://www.google.com/recaptcha/"), "test", "test", "test", "test", 1);

            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void ReCaptchaV2ConsidersAutoThemeArgument(bool autoTheme, bool expectScript)
        {
            // Arrange
            var baseUrl = new Uri("https://www.google.com/recaptcha/");
            const string siteKey = "test";
            const string size = "test";
            const string theme = "test";
            const string language = "test";
            const string callback = "test";
            const string errorCallback = "test";
            const string expiredCallback = "test";

            // Act
            IHtmlContent htmlContent = ReCaptchaGenerator.ReCaptchaV2(baseUrl, siteKey, size, theme, language, 
                callback, errorCallback, expiredCallback, autoTheme);

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
}
