using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace AspNetCore.ReCaptcha
{
    class ReCaptchaService : IReCaptchaService
    {
        private readonly HttpClient _client;
        private readonly ReCaptchaSettings _reCaptchaSettings;

        public ReCaptchaService(HttpClient client, IOptions<ReCaptchaSettings> reCaptchaSettings)
        {
            _client = client;
            _reCaptchaSettings = reCaptchaSettings.Value;
        }

        /// <inheritdoc />
        public async Task<bool> VerifyAsync(string reCaptchaResponse)
        {
            var obj = await GetVerifyResponseAsync(reCaptchaResponse);

            if (_reCaptchaSettings.Version == ReCaptchaVersion.V3)
            {
                return obj.Success && obj.Score >= _reCaptchaSettings.ScoreThreshold;
            }

            return obj.Success;
        }

        /// <inheritdoc />
        public async Task<ReCaptchaResponse> GetVerifyResponseAsync(string reCaptchaResponse)
        {
            var body = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["secret"] = _reCaptchaSettings.SecretKey,
                ["response"] = reCaptchaResponse
            });

            var url = "https://";
            if (!_reCaptchaSettings.UseRecaptchaNet)
            {
                url += "www.google.com/recaptcha";
            }
            else
            {
                url += "www.recaptcha.net";
            }
            url += "/api/siteverify";

            var result = await _client.PostAsync(url, body);

            var stringResult = await result.Content.ReadAsStringAsync();

            var obj = JsonSerializer.Deserialize<ReCaptchaResponse>(stringResult);

            return obj;
        }
    }
}
