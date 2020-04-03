using System.Threading.Tasks;

namespace AspNetCore.ReCaptcha
{
    public interface IReCaptchaService
    {
        Task<bool> Verify(string reCaptchaResponse);
    }
}