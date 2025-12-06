using System;
using System.IO;
using Kiosk.Data;
using Kiosk.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kiosk.Pages.Admin
{
    public class AddCategoryModel : PageModel
    {
        private readonly KioskDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public AddCategoryModel(KioskDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Category Category { get; set; }
        [BindProperty]
        public IFormFile ImageFile { get; set; }

        public string Message { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (ImageFile == null || ImageFile.Length == 0)
            {
                ModelState.AddModelError(nameof(ImageFile), "Plik obrazu jest wymagany.");
            }
            if (!ModelState.IsValid) return Page();

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "categories");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                ImageFile.CopyTo(stream);
            }

            Category.Image = $"/images/categories/{fileName}";


            _context.Categories.Add(Category);
            _context.SaveChanges();
            Message = "Kategoria dodana!";
            Category = new Category();
            return Page();
        }
    }
}
