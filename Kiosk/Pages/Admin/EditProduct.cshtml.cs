using System;
using System.IO;
using System.Linq;
using Kiosk.Data;
using Kiosk.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kiosk.Pages.Admin
{
    public class EditProductModel : PageModel
    {
        private readonly KioskDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EditProductModel(KioskDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public MenuItem Product { get; set; }

        [BindProperty]
        public string DefaultIngredientsInput { get; set; }

        [BindProperty]
        public string OptionalIngredientsInput { get; set; }

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public SelectList CategorySelectList { get; set; }

        public IActionResult OnGet(int id)
        {
            Product = _context.MenuItems.FirstOrDefault(m => m.Id == id);
            if (Product == null) return RedirectToPage("Index");

            CategorySelectList = new SelectList(_context.Categories, "Id", "Name", Product.CategoryId);

            DefaultIngredientsInput = string.Join("\n", _context.MenuItemIngredients
                .Where(i => i.MenuItemId == id && i.IsDefault)
                .Select(i => i.Name));

            OptionalIngredientsInput = string.Join("\n", _context.MenuItemIngredients
                .Where(i => i.MenuItemId == id && !i.IsDefault)
                .Select(i => i.Name));

            return Page();
        }

        public IActionResult OnPost()
        {
            CategorySelectList = new SelectList(_context.Categories, "Id", "Name", Product?.CategoryId);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var productInDb = _context.MenuItems.FirstOrDefault(m => m.Id == Product.Id);
            if (productInDb == null)
                return RedirectToPage("Index");

            var originalImagePath = productInDb.Image;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                productInDb.Image = $"/images/products/{fileName}";
            }

            productInDb.Name = Product.Name;
            productInDb.CategoryId = Product.CategoryId;
            productInDb.Price = Product.Price;
            productInDb.Description = Product.Description;

            if (ImageFile == null || ImageFile.Length == 0)
            {
                productInDb.Image = Product.Image;
            }

            if (!string.IsNullOrWhiteSpace(originalImagePath) && originalImagePath != productInDb.Image)
            {
                var physicalPath = Path.Combine(_environment.WebRootPath, originalImagePath.TrimStart('/')
                    .Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }

            var existingIngredients = _context.MenuItemIngredients.Where(i => i.MenuItemId == Product.Id);
            _context.MenuItemIngredients.RemoveRange(existingIngredients);

            var defaults = DefaultIngredientsInput?.Split('\n', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(name => new MenuItemIngredient { MenuItemId = Product.Id, Name = name, IsDefault = true });

            var optionals = OptionalIngredientsInput?.Split('\n', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(name => new MenuItemIngredient { MenuItemId = Product.Id, Name = name, IsDefault = false });

            if (defaults != null)
            {
                _context.MenuItemIngredients.AddRange(defaults);
            }

            if (optionals != null)
            {
                _context.MenuItemIngredients.AddRange(optionals);
            }

            _context.SaveChanges();

            return RedirectToPage("Index");
        }
    }
}