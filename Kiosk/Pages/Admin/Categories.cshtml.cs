using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Kiosk.Data;
using Kiosk.Models;
using System.Collections.Generic;
using System.Linq;

namespace Kiosk.Pages.Admin
{
    public class CategoriesModel : PageModel
    {
        private readonly KioskDbContext _context;

        public CategoriesModel(KioskDbContext context)
        {
            _context = context;
        }

        public List<Category> Categories { get; set; } = new();

        public void OnGet()
        {
            Categories = _context.Categories.ToList();
        }

        public IActionResult OnPostDelete(int CategoryId)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == CategoryId);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
            return RedirectToPage();
        }
    }
}
