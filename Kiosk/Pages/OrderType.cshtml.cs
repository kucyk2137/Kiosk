using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kiosk.Pages
{
    public class OrderTypeModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPost(string orderType)
        {
            if (string.IsNullOrWhiteSpace(orderType))
            {
                ModelState.AddModelError(string.Empty, "Wybierz rodzaj zamówienia.");
                return Page();
            }

            HttpContext.Session.SetString("OrderType", orderType);
            return RedirectToPage("/Menu");
        }
    }
}