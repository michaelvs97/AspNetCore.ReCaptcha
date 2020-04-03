using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCore.ReCaptcha.Models;
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

        public async Task<bool> Verify(string reCaptchaResponse)
        {
            var body = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["secret"] = _reCaptchaSettings.SecretKey,
                ["response"] = reCaptchaResponse
            });

            var result = await _client.PostAsync("https://www.google.com/recaptcha/api/siteverify", body);

            var stringResult = await result.Content.ReadAsStringAsync();

            var obj = JsonSerializer.Deserialize<ReCaptchaResponse>(stringResult);

            return obj.Success;
        }
    }
}
