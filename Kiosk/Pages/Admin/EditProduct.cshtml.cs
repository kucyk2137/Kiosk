using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using System.Linq;

namespace Kiosk.Pages.Admin
{
    public class EditProductModel : PageModel
    {
        private readonly KioskDbContext _context;

        public EditProductModel(KioskDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MenuItem Product { get; set; }

        public IActionResult OnGet(int id)
        {
            Product = _context.MenuItems.FirstOrDefault(m => m.Id == id);
            if (Product == null)
                return RedirectToPage("Index");

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            var productInDb = _context.MenuItems.FirstOrDefault(m => m.Id == Product.Id);
            if (productInDb == null)
                return RedirectToPage("Index");

            productInDb.Name = Product.Name;
            productInDb.Category = Product.Category;
            productInDb.Price = Product.Price;
            productInDb.Description = Product.Description;
            productInDb.ImageUrl = Product.ImageUrl;

            _context.SaveChanges();

            return RedirectToPage("Index");
        }
    }
}
