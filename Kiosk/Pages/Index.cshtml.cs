using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kiosk.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Przekierowanie do strony LockScreen
            return RedirectToPage("/LockScreen");
        }
    }
}
