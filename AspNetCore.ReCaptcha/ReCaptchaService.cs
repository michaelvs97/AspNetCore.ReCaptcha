using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore.ReCaptcha
{
    internal class ReCaptchaService : IReCaptchaService
    {
        private readonly HttpClient _client;
        private readonly ILogger<ReCaptchaService> _logger;
        private readonly ReCaptchaSettings _reCaptchaSettings;

        public ReCaptchaService(HttpClient client, IOptionsSnapshot<ReCaptchaSettings> reCaptchaSettings, ILogger<ReCaptchaService> logger)
        {
            _client = client;
            _logger = logger;
            _reCaptchaSettings = reCaptchaSettings.Value;
        }

        /// <inheritdoc />
        public async Task<bool> VerifyAsync(string reCaptchaResponse, string action = null)
        {
            var obj = await GetVerifyResponseAsync(reCaptchaResponse);

            if (_reCaptchaSettings.Version == ReCaptchaVersion.V3)
            {
                if (!obj.Success)
                    return false;

                if (!string.IsNullOrEmpty(action))
                {
                    if (action != obj.Action)
                        return false;

                    if (_reCaptchaSettings.ActionThresholds.TryGetValue(action, out var threshold))
                        return obj.Score >= threshold;
                }

                return obj.Score >= _reCaptchaSettings.ScoreThreshold;
            }

            return obj.Success;
        }

        /// <inheritdoc />
        public async Task<ReCaptchaResponse> GetVerifyResponseAsync(string reCaptchaResponse)
        {
            var body = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["secret"] = _reCaptchaSettings.SecretKey,
                ["response"] = reCaptchaResponse,
            });

            var result = await _client.PostAsync("api/siteverify", body);

            var stringResult = await result.Content.ReadAsStringAsync();
            _logger?.LogTrace("recaptcha response: {recaptchaResponse}", stringResult);

            var obj = JsonSerializer.Deserialize<ReCaptchaResponse>(stringResult);

            if (obj.ErrorCodes?.Length > 0 && _logger?.IsEnabled(LogLevel.Warning) == true)
            {
                for (var i = 0; i < obj.ErrorCodes.Length; i++)
                {
                    var errorCode = obj.ErrorCodes[i];
                    if (errorCode.EndsWith("-input-secret"))
                        _logger?.LogWarning("recaptcha verify returned error code {ErrorCode}, this could indicate an invalid secretkey.", errorCode);
                    else if (errorCode.EndsWith("-input-response"))
                        _logger?.LogDebug("recaptcha verify returned error code {ErrorCode}, this indicates the user didn't succeed the captcha.", errorCode);
                    else
                        _logger?.LogDebug("recaptcha verify returned error code {ErrorCode}.", errorCode);
                }
            }

            return obj;
        }
    }
}
