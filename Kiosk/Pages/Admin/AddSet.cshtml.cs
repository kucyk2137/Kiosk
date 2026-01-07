using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using Kiosk.Data;
using Kiosk.Models;
using Kiosk.Extensions;
using Kiosk.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace Kiosk.Pages.Admin
{
    public class AddSetModel : PageModel
    {
        private const string SetsCategoryName = "Zestawy";
        private readonly KioskDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AddSetModel(KioskDbContext context, IWebHostEnvironment environment, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _environment = environment;
            _localizer = localizer;
        }

        [BindProperty]
        [Required(ErrorMessage = "Nazwa zestawu jest wymagana.")]
        public string Name { get; set; }

        [BindProperty]
        public string NameEn { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public string DescriptionEn { get; set; }

        [BindProperty]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi być większa od zera.")]
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
                ModelState.AddModelError(nameof(SelectedProductIds), _localizer["Zestaw musi zawierać minimum 2 produkty."]);
            }

            if (!ModelState.IsValid)
            {
                Error = _localizer["Popraw błędy w formularzu."];
                return Page();
            }

            var setsCategory = EnsureSetsCategory();

            var imagePath = ResolveImagePath();
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                Error = _localizer["Nie udało się ustawić obrazu zestawu."];
                return Page();
            }

            var finalDescription = string.IsNullOrWhiteSpace(Description)
                ? BuildAutoDescription(new CultureInfo("pl-PL"), "Zestaw zawiera:")
                : Description;

            var finalDescriptionEn = string.IsNullOrWhiteSpace(DescriptionEn)
                ? BuildAutoDescription(new CultureInfo("en-US"), "Set includes:")
                : DescriptionEn;

            var setMenuItem = new MenuItem
            {
                Name = Name,
                NameEn = NameEn,
                Description = finalDescription,
                DescriptionEn = finalDescriptionEn,
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

            Message = _localizer["Zestaw został dodany."];
            ClearForm();
            LoadAvailableProducts();
            return Page();
        }

        private void LoadAvailableProducts()
        {
            var setsCategoryName = SetsCategoryName.ToLower();
            var setsCategoryNameEn = "sets";

            AvailableProducts = _context.MenuItems
                .Include(m => m.Category)
                .Where(m => m.Category != null
                            && m.Category.Name.ToLower() != setsCategoryName
                            && (m.Category.NameEn == null || m.Category.NameEn.ToLower() != setsCategoryNameEn))
                .OrderBy(m => m.Name)
                .ToList();
        }

        private Category EnsureSetsCategory()
        {
            var fallbackImage = AvailableProducts
                .FirstOrDefault(p => SelectedProductIds.Contains(p.Id))?.Image ?? string.Empty;

            var setsCategoryName = SetsCategoryName.ToLower();
            var setsCategoryNameEn = "sets";

            var category = _context.Categories
                .FirstOrDefault(c => c.Name.ToLower() == setsCategoryName || (c.NameEn != null && c.NameEn.ToLower() == setsCategoryNameEn));

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
                NameEn = "Sets",
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

        private string BuildAutoDescription(CultureInfo culture, string prefix)
        {
            var names = AvailableProducts
                .Where(p => SelectedProductIds.Contains(p.Id))
                .Select(p => p.GetDisplayName(culture))
                .ToList();

            return names.Any()
                ? $"{prefix} {string.Join(", ", names)}"
                : string.Empty;
        }

        private void ClearForm()
        {
            Name = string.Empty;
            NameEn = string.Empty;
            Description = string.Empty;
            DescriptionEn = string.Empty;
            Price = 0;
            SelectedProductIds = new List<int>();
            ImageFile = null;
        }
    }
}
