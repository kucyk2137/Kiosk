using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using System.Collections.Generic;
using System.Linq;

namespace Kiosk.Pages.Categories
{
    public class DrinksModel : PageModel
    {
        private readonly KioskDbContext _context;

        public DrinksModel(KioskDbContext context)
        {
            _context = context;
        }

        // Lista napojów dla widoku
        public List<MenuItem> Products { get; set; } = new();

        // Jedyna metoda OnGet
        public void OnGet()
        {
            Products = _context.MenuItems
                               .Where(m => m.Category == "Drinks")
                               .ToList();
        }
    }
}
