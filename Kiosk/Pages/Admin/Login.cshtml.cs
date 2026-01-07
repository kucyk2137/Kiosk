using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Http;
using Kiosk.Resources;

namespace Kiosk.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        public LoginModel(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // Prosta weryfikacja admina (login i hasło na stałe)
            if (Username == "admin" && Password == "admin123")
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToPage("/Admin/Index");
            }

            ErrorMessage = "Nieprawidłowy login lub hasło";
            return Page();
        }
    }
}
