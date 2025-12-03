using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;

namespace Kiosk.Pages.Categories
{
    public class PizzaModel : PageModel
    {
        private readonly KioskDbContext _context;

        public PizzaModel(KioskDbContext context)
        {
            _context = context;
        }

        public List<MenuItem> MenuItems { get; set; } = new();

        public void OnGet()
        {
            MenuItems = _context.MenuItems
                        .Where(m => m.Category == "Pizza")
                        .ToList();
        }
    }
}
