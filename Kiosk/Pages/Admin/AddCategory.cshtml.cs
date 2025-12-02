using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Kiosk.Data;
using Kiosk.Models;

namespace Kiosk.Pages.Admin
{
    public class AddCategoryModel : PageModel
    {
        private readonly KioskDbContext _context;
        public AddCategoryModel(KioskDbContext context) => _context = context;

        [BindProperty]
        public Category Category { get; set; }

        public string Message { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            _context.Categories.Add(Category);
            _context.SaveChanges();
            Message = "Kategoria dodana!";
            Category = new Category();
            return Page();
        }
    }
}
