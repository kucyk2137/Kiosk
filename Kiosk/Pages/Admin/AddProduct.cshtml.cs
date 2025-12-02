using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;

namespace Kiosk.Pages.Admin
{
    public class AddProductModel : PageModel
    {
        private readonly KioskDbContext _context;

        public AddProductModel(KioskDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MenuItem MenuItem { get; set; }

        public string Message { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToPage("/Admin/Login");

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.MenuItems.Add(MenuItem);
            _context.SaveChanges();

            Message = "Produkt zosta³ dodany!";
            MenuItem = new MenuItem(); // wyczyœæ formularz
            return Page();
        }
    }
}
