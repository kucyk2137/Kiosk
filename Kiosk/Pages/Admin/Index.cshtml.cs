using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;

namespace Kiosk.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly KioskDbContext _context;

        public IndexModel(KioskDbContext context)
        {
            _context = context;
        }

        public List<MenuItem> MenuItems { get; set; } = new();

        public IActionResult OnGet()
        {
            // Sprawdzenie, czy admin jest zalogowany
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToPage("/Admin/Login");

            MenuItems = _context.MenuItems.ToList();
            return Page();
        }
    }
}
