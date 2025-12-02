using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using System.Collections.Generic;
using System.Linq;

namespace Kiosk.Pages.Categories
{
    public class DessertsModel : PageModel
    {
        private readonly KioskDbContext _context;

        public DessertsModel(KioskDbContext context)
        {
            _context = context;
        }

        // Lista produktów dla widoku
        public List<MenuItem> Products { get; set; } = new();

        // Jedyna metoda OnGet
        public void OnGet()
        {
            Products = _context.MenuItems
                               .Where(m => m.Category == "Desserts")
                               .ToList();
        }
    }
}
