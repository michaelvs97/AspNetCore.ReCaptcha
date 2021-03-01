using AspNetCore.ReCaptcha.Net50.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.ReCaptcha.Net50.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [ValidateReCaptcha]
        [HttpPost]
        public IActionResult SubmitForm(ContactViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index");

            TempData["Message"] = "Your form has been sent!";
            return RedirectToAction("Index");
        }
    }
}