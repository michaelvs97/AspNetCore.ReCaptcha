using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Localization;

namespace AspNetCore.ReCaptcha
{
    [ExcludeFromCodeCoverage]
    public class ReCaptchaSettings
    {
        internal static readonly Uri GoogleReCaptchaBaseUrl = new Uri("https://www.google.com/recaptcha/");
        internal static readonly Uri RecaptchaNetBaseUrl = new Uri("https://www.recaptcha.net/recaptcha/");

        public string SiteKey { get; set; }
        public string SecretKey { get; set; }
        public ReCaptchaVersion Version { get; set; }
        public bool UseRecaptchaNet { get; set; }
        public double ScoreThreshold { get; set; } = 0.5;
        public Dictionary<string, double> ActionThresholds { get; set; } = new Dictionary<string, double>();
        public Func<Type, IStringLocalizerFactory, IStringLocalizer> LocalizerProvider { get; set; }
        public Uri RecaptchaBaseUrl { get; set; } = GoogleReCaptchaBaseUrl;

        /// <summary>
        /// Determines whether Google reCAPTCHA is enabled / enforced. Defaults to <code>true</code> for backward
        /// compatibility with existing application deployments.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}
