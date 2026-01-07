using System;
using System.IO;
using System.Linq;
using Kiosk.Data;
using Kiosk.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kiosk.Pages.Admin
{
    public class EditCategoryModel : PageModel
    {
        private readonly KioskDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EditCategoryModel(KioskDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Category Category { get; set; }

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

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

            var originalImagePath = categoryInDb.Image;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "categories");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                categoryInDb.Image = $"/images/categories/{fileName}";
            }

            categoryInDb.Name = Category.Name;
            categoryInDb.NameEn = Category.NameEn;

            if (ImageFile == null || ImageFile.Length == 0)
            {
                categoryInDb.Image = Category.Image;
            }

            if (!string.IsNullOrWhiteSpace(originalImagePath) && originalImagePath != categoryInDb.Image)
            {
                var physicalPath = Path.Combine(_environment.WebRootPath, originalImagePath.TrimStart('/')
                    .Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            _context.SaveChanges();

            return RedirectToPage("Categories");
        }
    }
}