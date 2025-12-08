using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kiosk.Data;
using Kiosk.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Kiosk.Pages.Admin
{
    public class AddSetModel : PageModel
    {
        private const string SetsCategoryName = "Zestawy";
        private readonly KioskDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AddSetModel(KioskDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        [Required(ErrorMessage = "Nazwa zestawu jest wymagana.")]
        public string Name { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi byæ wiêksza od zera.")]
        public decimal Price { get; set; }

        [BindProperty]
        public List<int> SelectedProductIds { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public List<MenuItem> AvailableProducts { get; set; } = new();

        public string? Message { get; set; }
        public string? Error { get; set; }

        public void OnGet()
        {
            LoadAvailableProducts();
        }

        public IActionResult OnPost()
        {
            LoadAvailableProducts();

            if (SelectedProductIds == null)
            {
                SelectedProductIds = new List<int>();
            }

            if (SelectedProductIds.Count < 2)
            {
                ModelState.AddModelError(nameof(SelectedProductIds), "Zestaw musi zawieraæ minimum 2 produkty.");
            }

            if (!ModelState.IsValid)
            {
                Error = "Popraw b³êdy w formularzu.";
                return Page();
            }

            var setsCategory = EnsureSetsCategory();

            var imagePath = ResolveImagePath();
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                Error = "Nie uda³o siê ustawiæ obrazu zestawu.";
                return Page();
            }

            var finalDescription = string.IsNullOrWhiteSpace(Description)
                ? BuildAutoDescription()
                : Description;

            var setMenuItem = new MenuItem
            {
                Name = Name,
                Description = finalDescription,
                Price = Price,
                Image = imagePath,
                CategoryId = setsCategory.Id
            };

            _context.MenuItems.Add(setMenuItem);
            _context.SaveChanges();

            var productSet = new ProductSet
            {
                SetMenuItemId = setMenuItem.Id
            };

            _context.ProductSets.Add(productSet);
            _context.SaveChanges();

            var setItems = SelectedProductIds.Select(id => new ProductSetItem
            {
                ProductSetId = productSet.Id,
                MenuItemId = id
            });

            _context.ProductSetItems.AddRange(setItems);
            _context.SaveChanges();

            Message = "Zestaw zosta³ dodany.";
            ClearForm();
            LoadAvailableProducts();
            return Page();
        }

        private void LoadAvailableProducts()
        {
            var setsCategoryName = SetsCategoryName.ToLower();

            AvailableProducts = _context.MenuItems
                .Include(m => m.Category)
                .Where(m => m.Category != null
                            && m.Category.Name.ToLower() != setsCategoryName)
                .OrderBy(m => m.Name)
                .ToList();
        }

        private Category EnsureSetsCategory()
        {
            var fallbackImage = AvailableProducts
                .FirstOrDefault(p => SelectedProductIds.Contains(p.Id))?.Image ?? string.Empty;

            var setsCategoryName = SetsCategoryName.ToLower();

            var category = _context.Categories
                .FirstOrDefault(c => c.Name.ToLower() == setsCategoryName);

            if (category != null)
            {
                if (string.IsNullOrWhiteSpace(category.Image) && !string.IsNullOrWhiteSpace(fallbackImage))
                {
                    category.Image = fallbackImage;
                    _context.SaveChanges();
                }
                return category;
            }

            category = new Category
            {
                Name = SetsCategoryName,
                Image = fallbackImage
            };

            _context.Categories.Add(category);
            _context.SaveChanges();

            return category;
        }

        private string ResolveImagePath()
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                ImageFile.CopyTo(stream);

                return $"/images/products/{fileName}";
            }

            var firstSelected = AvailableProducts.FirstOrDefault(p => SelectedProductIds.Contains(p.Id));
            return firstSelected?.Image ?? string.Empty;
        }

        private string BuildAutoDescription()
        {
            var names = AvailableProducts
                .Where(p => SelectedProductIds.Contains(p.Id))
                .Select(p => p.Name)
                .ToList();

            return names.Any()
                ? $"Zestaw zawiera: {string.Join(", ", names)}"
                : string.Empty;
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Description = string.Empty;
            Price = 0;
            SelectedProductIds = new List<int>();
            ImageFile = null;
        }
    }
}