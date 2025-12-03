using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using System.Collections.Generic;
using System.Linq;

namespace Kiosk.Pages.Categories
{
    public class BurgersModel : PageModel
    {
        private readonly KioskDbContext _context;

        public BurgersModel(KioskDbContext context)
        {
            _context = context;
        }

        // Lista burgerów dla widoku
        public List<MenuItem> Burgers { get; set; } = new();

        public void OnGet()
        {
            Burgers = _context.MenuItems
                              .Where(m => m.Category == "Burgery")
                              .ToList();
        }
    }
}
