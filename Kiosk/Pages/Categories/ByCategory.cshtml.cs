using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using System.Collections.Generic;
using System.Linq;

namespace Kiosk.Pages.Categories
{
    public class ByCategoryModel : PageModel
    {
        private readonly KioskDbContext _context;

        public ByCategoryModel(KioskDbContext context)
        {
            _context = context;
        }

        public Category Category { get; set; }
        public List<MenuItem> Products { get; set; } = new();

        public IActionResult OnGet(int categoryId)
        {
            Category = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (Category == null)
                return RedirectToPage("/Menu");

            Products = _context.MenuItems
                               .Where(m => m.CategoryId == categoryId)
                               .ToList();

            return Page();
        }
    }
}
