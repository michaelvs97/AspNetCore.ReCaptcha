using System.Diagnostics.CodeAnalysis;

namespace AspNetCore.ReCaptcha
{
    [ExcludeFromCodeCoverage]
    public class ReCaptchaSettings
    {
        public string SiteKey { get; set; }
        public string SecretKey { get; set; }
        public ReCaptchaVersion Version { get; set; }
        public bool UseRecaptchaNet { get; set; }
        public float ScoreThreshold { get; set; } = 0.5f;
    }
}
