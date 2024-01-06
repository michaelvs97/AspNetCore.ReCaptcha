using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace AspNetCore.ReCaptcha.Tests
{
    public class ReCaptchaServiceTests
    {
        private ReCaptchaService CreateService(HttpClient httpClient = null, ReCaptchaSettings reCaptchaSettings = null, ILogger<ReCaptchaService> logger = null)
        {
            httpClient ??= new HttpClient();

            if (reCaptchaSettings == null)
            {
                reCaptchaSettings = new ReCaptchaSettings()
                {
                    SecretKey = "123",
                    SiteKey = "123",
                    Version = ReCaptchaVersion.V2
                };
            }

            var reCaptchaSettingsMock = new Mock<IOptionsSnapshot<ReCaptchaSettings>>();
            reCaptchaSettingsMock.Setup(x => x.Value).Returns(reCaptchaSettings);

            logger ??= new NullLogger<ReCaptchaService>();

            return new ReCaptchaService(httpClient, reCaptchaSettingsMock.Object, logger);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestVerifyAsync(bool successResult)
        {
            var reCaptchaResponse = new ReCaptchaResponse()
            {
                Action = "Test",
                ChallengeTimestamp = DateTime.Now,
                Hostname = "Test",
                Score = 1.0,
                Success = successResult
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(reCaptchaResponse), Encoding.UTF8,"application/json")});

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://www.google.com/recaptcha/"),
            };

            var reCaptchaService = CreateService(httpClient);

            var result = reCaptchaService.VerifyAsync("123").Result;

            mockHttpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Assert.Equal(successResult, result);
        }

        [Theory]
        [InlineData("missing-input-secret", LogLevel.Warning, "recaptcha verify returned error code missing-input-secret, this could indicate an invalid secretkey.")]
        [InlineData("invalid-input-secret", LogLevel.Warning, "recaptcha verify returned error code invalid-input-secret, this could indicate an invalid secretkey.")]
        [InlineData("missing-input-response", LogLevel.Debug, "recaptcha verify returned error code missing-input-response, this indicates the user didn't succeed the captcha.")]
        [InlineData("invalid-input-response", LogLevel.Debug, "recaptcha verify returned error code invalid-input-response, this indicates the user didn't succeed the captcha.")]
        [InlineData("bad-request", LogLevel.Debug, "recaptcha verify returned error code bad-request.")]
        [InlineData("timeout-or-duplicate", LogLevel.Debug, "recaptcha verify returned error code timeout-or-duplicate.")]
        public void TestVerifyWithErrorAsync(string errorCode, LogLevel expectedLogLevel, string expectedLogMessage)
        {
            var reCaptchaResponse = new ReCaptchaResponse()
            {
                Action = "Test",
                ChallengeTimestamp = new DateTime(2022, 2, 10, 15, 14, 13),
                Hostname = "Test",
                Success = false,
                ErrorCodes = new[] { errorCode }
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(reCaptchaResponse), Encoding.UTF8,"application/json")});

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://www.google.com/recaptcha/"),
            };

            var logger = new TestLogger<ReCaptchaService>();

            var reCaptchaService = CreateService(httpClient, logger: logger);

            var result = reCaptchaService.VerifyAsync("123").Result;

            mockHttpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Assert.False(reCaptchaResponse.Success);
            Assert.False(result);

            Assert.Equal(2, logger.LogEntries.Count);
            Assert.Equal(LogLevel.Trace, logger.LogEntries[0].LogLevel);
            Assert.Equal(@$"recaptcha response: {{""success"":false,""score"":0,""action"":""Test"",""challenge_ts"":""2022-02-10T15:14:13"",""hostname"":""Test"",""error-codes"":[""{errorCode}""]}}", logger.LogEntries[0].Message);

            Assert.Equal(expectedLogLevel, logger.LogEntries[1].LogLevel);
            Assert.Equal(expectedLogMessage, logger.LogEntries[1].Message);
        }

        [Fact]
        public void TestVerifyWithActionReturnsFalseIfInvalidAction()
        {
            var reCaptchaResponse = new ReCaptchaResponse()
            {
                Action = "Test",
                ChallengeTimestamp = new DateTime(2022, 2, 10, 15, 14, 13),
                Hostname = "Test",
                Success = true,
                Score = 1.0,
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(reCaptchaResponse), Encoding.UTF8,"application/json")});

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://www.google.com/recaptcha/"),
            };

            var logger = new TestLogger<ReCaptchaService>();

            var reCaptchaService = CreateService(httpClient, logger: logger, reCaptchaSettings: new ReCaptchaSettings
            {
                Version = ReCaptchaVersion.V3,
            });

            var result = reCaptchaService.VerifyAsync("123", "Test2").Result;

            mockHttpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Assert.False(result);
        }

        [Fact]
        public void TestVerifyWithActionReturnsFalseIfScoreLessThanActionThreshold()
        {
            var reCaptchaResponse = new ReCaptchaResponse()
            {
                Action = "Test",
                ChallengeTimestamp = new DateTime(2022, 2, 10, 15, 14, 13),
                Hostname = "Test",
                Success = true,
                Score = 0.7,
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(reCaptchaResponse), Encoding.UTF8,"application/json")});

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://www.google.com/recaptcha/"),
            };

            var logger = new TestLogger<ReCaptchaService>();

            var reCaptchaService = CreateService(httpClient, logger: logger, reCaptchaSettings: new ReCaptchaSettings
            {
                Version = ReCaptchaVersion.V3,
                ScoreThreshold = 0.5,
                ActionThresholds =
                {
                    ["Test"] = 0.8,
                },
            });

            var result = reCaptchaService.VerifyAsync("123", "Test").Result;

            mockHttpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Assert.False(result);
        }

        [Fact]
        public void TestVerifyWithAction()
        {
            var reCaptchaResponse = new ReCaptchaResponse()
            {
                Action = "Test",
                ChallengeTimestamp = new DateTime(2022, 2, 10, 15, 14, 13),
                Hostname = "Test",
                Success = true,
                Score = 1.0,
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(reCaptchaResponse), Encoding.UTF8,"application/json")});

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://www.google.com/recaptcha/"),
            };

            var logger = new TestLogger<ReCaptchaService>();

            var reCaptchaService = CreateService(httpClient, logger: logger, reCaptchaSettings: new ReCaptchaSettings
            {
                Version = ReCaptchaVersion.V3,
                ScoreThreshold = 0.5,
                ActionThresholds =
                {
                    ["Test"] = 0.8,
                },
            });

            var result = reCaptchaService.VerifyAsync("123", "Test").Result;

            mockHttpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Assert.True(result);
        }
    }
}
