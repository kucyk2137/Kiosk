using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Kiosk.Data;
using Kiosk.Models;
using Kiosk.Extensions;
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
        public string DefaultIngredientsInputEn { get; set; }

        [BindProperty]
        public string OptionalIngredientsInputEn { get; set; }

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public SelectList CategorySelectList { get; set; }

        public IActionResult OnGet(int id)
        {
            Product = _context.MenuItems.FirstOrDefault(m => m.Id == id);
            if (Product == null) return RedirectToPage("Index");

            var culture = CultureInfo.CurrentUICulture;
            CategorySelectList = new SelectList(_context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, Name = c.GetDisplayName(culture) }), "Id", "Name", Product.CategoryId);

            DefaultIngredientsInput = string.Join("\n", _context.MenuItemIngredients
                .Where(i => i.MenuItemId == id && i.IsDefault)
                .Select(i => i.Name));

            DefaultIngredientsInputEn = string.Join("\n", _context.MenuItemIngredients
                .Where(i => i.MenuItemId == id && i.IsDefault)
                .Select(i => i.NameEn ?? string.Empty));

            OptionalIngredientsInput = string.Join("\n", _context.MenuItemIngredients
                .Where(i => i.MenuItemId == id && !i.IsDefault)
                .Select(i => i.Name));

            OptionalIngredientsInputEn = string.Join("\n", _context.MenuItemIngredients
                .Where(i => i.MenuItemId == id && !i.IsDefault)
                .Select(i => FormatIngredientLine(i.NameEn, i.AdditionalPrice)));

            return Page();
        }

        public IActionResult OnPost()
        {
            var culture = CultureInfo.CurrentUICulture;
            CategorySelectList = new SelectList(_context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, Name = c.GetDisplayName(culture) }), "Id", "Name", Product?.CategoryId);

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
            productInDb.NameEn = Product.NameEn;
            productInDb.CategoryId = Product.CategoryId;
            productInDb.Price = Product.Price;
            productInDb.Description = Product.Description;
            productInDb.DescriptionEn = Product.DescriptionEn;

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

            var defaults = BuildIngredients(DefaultIngredientsInput, DefaultIngredientsInputEn, true, Product.Id);
            var optionals = BuildIngredients(OptionalIngredientsInput, OptionalIngredientsInputEn, false, Product.Id);

            if (defaults.Any())
            {
                _context.MenuItemIngredients.AddRange(defaults);
            }

            if (optionals.Any())
            {
                _context.MenuItemIngredients.AddRange(optionals);
            }

            _context.SaveChanges();

            return RedirectToPage("Index");
        }
        private static string FormatIngredientLine(string? name, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            return price > 0 ? $"{name} | {price:0.00}" : name;
        }

        private List<MenuItemIngredient> BuildIngredients(string input, string inputEn, bool isDefault, int menuItemId)
        {
            var lines = SplitLines(input);
            var linesEn = SplitLines(inputEn);
            var ingredients = new List<MenuItemIngredient>();

            for (var index = 0; index < lines.Count; index++)
            {
                var (name, price, hasPrice) = ParseIngredientLine(lines[index], !isDefault);
                var (nameEn, enPrice, enHasPrice) = ParseIngredientLine(index < linesEn.Count ? linesEn[index] : string.Empty, !isDefault);
                var finalPrice = hasPrice ? price : (enHasPrice ? enPrice : 0m);

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                ingredients.Add(new MenuItemIngredient
                {
                    MenuItemId = menuItemId,
                    Name = name,
                    NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn,
                    IsDefault = isDefault,
                    AdditionalPrice = finalPrice
                });
            }

            return ingredients;
        }

        private static List<string> SplitLines(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? new List<string>()
                : input
                    .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();
        }

        private static (string name, decimal price, bool hasPrice) ParseIngredientLine(string line, bool allowPrice)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return (string.Empty, 0m, false);
            }

            var parts = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var name = parts.FirstOrDefault()?.Trim() ?? string.Empty;

            if (!allowPrice || parts.Length < 2)
            {
                return (name, 0m, false);
            }

            var priceText = parts[1].Trim().Replace(',', '.');
            if (decimal.TryParse(priceText, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsed))
            {
                return (name, parsed, true);
            }

            return (name, 0m, false);
        }
    }
}