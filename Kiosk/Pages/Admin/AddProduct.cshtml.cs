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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

public class AddProductModel : PageModel
{
    private readonly KioskDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IStringLocalizer<SharedResource> _localizer;
    public AddProductModel(KioskDbContext context, IWebHostEnvironment environment, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _environment = environment;
        _localizer = localizer;
    }

    [BindProperty]
    public MenuItem MenuItem { get; set; }

    [BindProperty]
    public string DefaultIngredientsInput { get; set; }

    [BindProperty]
    public string OptionalIngredientsInput { get; set; }

    [BindProperty]
    public string DefaultIngredientsInputEn { get; set; }

    [BindProperty]
    public string OptionalIngredientsInputEn { get; set; }

    [BindProperty]
    public IFormFile ImageFile { get; set; }

    public SelectList CategorySelectList { get; set; }

    public string Message { get; set; }

    public void OnGet()
    {
        var culture = CultureInfo.CurrentUICulture;
        CategorySelectList = new SelectList(_context.Categories.OrderBy(c => c.Name)
            .Select(c => new { c.Id, Name = c.GetDisplayName(culture) }), "Id", "Name");
    }

    public IActionResult OnPost()
    {
        var culture = CultureInfo.CurrentUICulture;
        CategorySelectList = new SelectList(_context.Categories.OrderBy(c => c.Name)
            .Select(c => new { c.Id, Name = c.GetDisplayName(culture) }), "Id", "Name");

        ModelState.Remove("MenuItem.Image");

        if (!ModelState.IsValid)
        {
            var allErrors = ModelState
                .SelectMany(kvp => kvp.Value.Errors.Select(e => $"{kvp.Key}: {e.ErrorMessage}"))
                .ToList();

            Message = _localizer["Formularz zawiera błędy:"] + " " + string.Join(" | ", allErrors);
            return Page();
        }

        if (ImageFile == null || ImageFile.Length == 0)
        {
            ModelState.AddModelError(nameof(ImageFile), _localizer["Plik obrazu jest wymagany."]);
            return Page();
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
        Directory.CreateDirectory(uploadsFolder);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            ImageFile.CopyTo(stream);
        }

        MenuItem.Image = $"/images/products/{fileName}";

        _context.MenuItems.Add(MenuItem);
        _context.SaveChanges();

        var ingredientsToAdd = new List<MenuItemIngredient>();
        ingredientsToAdd.AddRange(ParseIngredients(DefaultIngredientsInput, DefaultIngredientsInputEn, true, MenuItem.Id));
        ingredientsToAdd.AddRange(ParseIngredients(OptionalIngredientsInput, OptionalIngredientsInputEn, false, MenuItem.Id));

        if (ingredientsToAdd.Any())
        {
            _context.MenuItemIngredients.AddRange(ingredientsToAdd);
            _context.SaveChanges();
        }

        Message = _localizer["Produkt został dodany!"];
        MenuItem = new MenuItem();
        DefaultIngredientsInput = string.Empty;
        OptionalIngredientsInput = string.Empty;
        DefaultIngredientsInputEn = string.Empty;
        OptionalIngredientsInputEn = string.Empty;
        return Page();
    }

    private IEnumerable<MenuItemIngredient> ParseIngredients(string input, string inputEn, bool isDefault, int menuItemId)
    {
        var lines = SplitLines(input);
        var linesEn = SplitLines(inputEn);

        if (!lines.Any())
        {
            return Enumerable.Empty<MenuItemIngredient>();
        }

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
