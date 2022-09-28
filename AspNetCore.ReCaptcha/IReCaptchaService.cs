using System.Threading.Tasks;

namespace AspNetCore.ReCaptcha
{
    public interface IReCaptchaService
    {
        /// <summary>
        /// Verifies provided ReCaptcha Response.
        /// </summary>
        /// <param name="reCaptchaResponse">ReCaptcha Response as given by the widget.</param>
        /// <returns>Returns whether the recaptcha validation was successful or not.</returns>
        Task<bool> VerifyAsync(string reCaptchaResponse, string action = null);

        /// <summary>
        /// Verifies provided ReCaptcha Response.
        /// </summary>
        /// <param name="reCaptchaResponse">ReCaptcha Response as given by the widget.</param>
        /// <returns>Returns result of the verification of the ReCaptcha Response.</returns>
        Task<ReCaptchaResponse> GetVerifyResponseAsync(string reCaptchaResponse);
    }
}
