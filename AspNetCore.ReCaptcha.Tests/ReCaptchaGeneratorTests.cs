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
    }
}
