using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kiosk.Data;
using Kiosk.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kiosk.Pages.Admin
{
    public class CategoriesModel : PageModel
    {
        private readonly KioskDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CategoriesModel(KioskDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
                if (!string.IsNullOrWhiteSpace(category.Image))
                {
                    var physicalPath = Path.Combine(_environment.WebRootPath, category.Image.TrimStart('/')
                        .Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }

                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
            return RedirectToPage();
        }
    }
}