using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using System.Linq;

namespace Kiosk.Pages.Admin
{
    public class EditCategoryModel : PageModel
    {
        private readonly KioskDbContext _context;
        public EditCategoryModel(KioskDbContext context) => _context = context;

        [BindProperty]
        public Category Category { get; set; }

        public IActionResult OnGet(int id)
        {
            Category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (Category == null) return RedirectToPage("Categories");
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            var categoryInDb = _context.Categories.FirstOrDefault(c => c.Id == Category.Id);
            if (categoryInDb == null) return RedirectToPage("Categories");

            categoryInDb.Name = Category.Name;
            _context.SaveChanges();

            return RedirectToPage("Categories");
        }
    }
}
