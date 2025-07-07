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
            var result = ReCaptchaGenerator.ReCaptchaV2(new Uri("https://www.google.com/recaptcha/"), "test", "test", "test", "test", "test", "test", "test", false, "nonce");

            Assert.NotNull(result);
            Assert.Equal(@"<div class=""g-recaptcha"" data-sitekey=""test"" data-size=""test"" data-theme=""test"" data-callback=""test"" data-error-callback=""test"" data-expired-callback=""test""></div>
<script src=""https://www.google.com/recaptcha/api.js?hl=test"" nonce=""nonce"" defer></script>",
                result.ToHtmlString());
        }

        [Fact]
        public void ReCaptchaGeneratorReturnsReCaptchaForV2Invisible()
        {
            var result = ReCaptchaGenerator.ReCaptchaV2Invisible(new Uri("https://www.google.com/recaptcha/"), "test", "test", "test", "test", "test", "test", "test", "test");

            Assert.NotNull(result);
            Assert.Equal(@"<button class=""g-recaptcha test"" data-sitekey=""test"" data-badge=""test"" data-callback=""test"" data-expired-callback=""test"" data-error-callback=""test"">test</button>
<script src=""https://www.google.com/recaptcha/api.js?hl=test"" defer></script>", result.ToHtmlString());
        }

        [Fact]
        public void ReCaptchaGeneratorWhenNonceNonNullReturnsReCaptchaForV3()
        {
            var result = ReCaptchaGenerator.ReCaptchaV3(new Uri("https://www.google.com/recaptcha/"), "test", "test", "test", "test", 1, "nonce&\"");

            Assert.NotNull(result);
            Assert.Equal(
                @"<input id=""g-recaptcha-response-1"" name=""g-recaptcha-response"" type=""hidden"" value="""" /><script src=""https://www.google.com/recaptcha/api.js?render=test&hl=test"" nonce=""nonce&amp;&quot;""></script><script nonce=""nonce&amp;&quot;"">function updateReCaptcha1() {grecaptcha.execute('test', {action: 'test'}).then(function(token){document.getElementById('g-recaptcha-response-1').value = token;});}grecaptcha.ready(function() {setInterval(updateReCaptcha1, 100000); updateReCaptcha1()});</script>
",
                result.ToHtmlString());
        }

        [Fact]
        public void ReCaptchaGeneratorWhenNonceNullReturnsReCaptchaForV3()
        {
            var result = ReCaptchaGenerator.ReCaptchaV3(new Uri("https://www.google.com/recaptcha/"), "test", "test", "test", "test", 1, null);

            Assert.NotNull(result);
            Assert.Equal(
                @"<input id=""g-recaptcha-response-1"" name=""g-recaptcha-response"" type=""hidden"" value="""" /><script src=""https://www.google.com/recaptcha/api.js?render=test&hl=test""></script><script>function updateReCaptcha1() {grecaptcha.execute('test', {action: 'test'}).then(function(token){document.getElementById('g-recaptcha-response-1').value = token;});}grecaptcha.ready(function() {setInterval(updateReCaptcha1, 100000); updateReCaptcha1()});</script>
",
                result.ToHtmlString());
        }

        [Fact]
        public void ReCaptchaGeneratorWhenNonceEmptyReturnsReCaptchaForV3()
        {
            var result = ReCaptchaGenerator.ReCaptchaV3(new Uri("https://www.google.com/recaptcha/"), "test", "test", "test", "test", 1, "");

            Assert.NotNull(result);
            Assert.Equal(
                @"<input id=""g-recaptcha-response-1"" name=""g-recaptcha-response"" type=""hidden"" value="""" /><script src=""https://www.google.com/recaptcha/api.js?render=test&hl=test""></script><script>function updateReCaptcha1() {grecaptcha.execute('test', {action: 'test'}).then(function(token){document.getElementById('g-recaptcha-response-1').value = token;});}grecaptcha.ready(function() {setInterval(updateReCaptcha1, 100000); updateReCaptcha1()});</script>
",
                result.ToHtmlString());
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
