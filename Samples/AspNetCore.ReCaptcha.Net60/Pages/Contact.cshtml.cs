using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspNetCore.ReCaptcha.Net60.Pages;

[ValidateReCaptcha("contact")]
public class ContactModel : PageModel
{
    [BindProperty]
    public string Name { get; set; }

    [BindProperty]
    public string Body { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        TempData["Message"] = "Your form has been sent!";
        return RedirectToPage();
    }
}
