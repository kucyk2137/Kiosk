using System;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kiosk.Pages
{
    public class SetLanguageModel : PageModel
    {
        public IActionResult OnPost(string culture, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                culture = "pl-PL";
            }

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Content("~");
            }

            return LocalRedirect(returnUrl);
        }
    }
}
