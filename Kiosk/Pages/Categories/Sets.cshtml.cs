using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using System.Collections.Generic;
using System.Linq;

namespace Kiosk.Pages.Categories
{
    public class SetsModel : PageModel
    {
        private readonly KioskDbContext _context;

        public SetsModel(KioskDbContext context) => _context = context;

        public List<MenuItem> Products { get; set; } = new();

        public void OnGet()
        {
            Products = _context.MenuItems
                               .Where(m => m.Category == "Sets")
                               .ToList();
        }
    }
}
