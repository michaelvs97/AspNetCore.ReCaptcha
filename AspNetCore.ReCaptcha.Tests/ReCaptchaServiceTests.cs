using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace AspNetCore.ReCaptcha.Tests
{
    public class ReCaptchaServiceTests
    {
        private ReCaptchaService CreateService(HttpClient httpClient = null, Mock<IOptions<ReCaptchaSettings>> reCaptchaSettingsMock = null)
        {
            httpClient ??= new HttpClient();

            if (reCaptchaSettingsMock == null)
            {
                var reCaptchaSettings = new ReCaptchaSettings()
                {
                    SecretKey = "123",
                    SiteKey = "123",
                    Version = ReCaptchaVersion.V2
                };

                reCaptchaSettingsMock = new Mock<IOptions<ReCaptchaSettings>>();
                reCaptchaSettingsMock.Setup(x => x.Value).Returns(reCaptchaSettings);
            }
            
            return new ReCaptchaService(httpClient, reCaptchaSettingsMock.Object);
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
                Score = decimal.One,
                Success = successResult
            };
            
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(reCaptchaResponse), Encoding.UTF8,"application/json")});

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            
            var reCaptchaService = CreateService(httpClient);

            var result = reCaptchaService.VerifyAsync("123").Result;

            mockHttpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            Assert.Equal(reCaptchaResponse.Success, result);
        }
    }
}
