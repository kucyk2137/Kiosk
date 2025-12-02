using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using System.Collections.Generic;
using System.Linq;

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

        public void OnGet()
        {
            MenuItems = _context.MenuItems.ToList();
        }

        public IActionResult OnPostDelete(int MenuItemId)
        {
            var item = _context.MenuItems.FirstOrDefault(m => m.Id == MenuItemId);
            if (item != null)
            {
                _context.MenuItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToPage();
        }
    }
}
