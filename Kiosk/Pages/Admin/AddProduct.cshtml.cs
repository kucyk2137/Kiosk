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
using Microsoft.AspNetCore.Mvc.Rendering;

public class AddProductModel : PageModel
{
    private readonly KioskDbContext _context;
    private readonly IWebHostEnvironment _environment;
    public AddProductModel(KioskDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public MenuItem MenuItem { get; set; }

    [BindProperty]
    public string DefaultIngredientsInput { get; set; }

    [BindProperty]
    public string OptionalIngredientsInput { get; set; }

    [BindProperty]
    public IFormFile ImageFile { get; set; }

    public SelectList CategorySelectList { get; set; }

    public string Message { get; set; }

    public void OnGet()
    {
        CategorySelectList = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");
    }

    public IActionResult OnPost()
    {
        CategorySelectList = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");

        ModelState.Remove("MenuItem.Image");

        if (!ModelState.IsValid)
        {
            var allErrors = ModelState
                .SelectMany(kvp => kvp.Value.Errors.Select(e => $"{kvp.Key}: {e.ErrorMessage}"))
                .ToList();

            Message = "Formularz zawiera b³êdy: " + string.Join(" | ", allErrors);
            return Page();
        }

        if (ImageFile == null || ImageFile.Length == 0)
        {
            ModelState.AddModelError(nameof(ImageFile), "Plik obrazu jest wymagany.");
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
        ingredientsToAdd.AddRange(ParseIngredients(DefaultIngredientsInput, true, MenuItem.Id));
        ingredientsToAdd.AddRange(ParseIngredients(OptionalIngredientsInput, false, MenuItem.Id));

        if (ingredientsToAdd.Any())
        {
            _context.MenuItemIngredients.AddRange(ingredientsToAdd);
            _context.SaveChanges();
        }

        Message = "Produkt zosta³ dodany!";
        MenuItem = new MenuItem();
        DefaultIngredientsInput = string.Empty;
        OptionalIngredientsInput = string.Empty;
        return Page();
    }

    private IEnumerable<MenuItemIngredient> ParseIngredients(string input, bool isDefault, int menuItemId)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Enumerable.Empty<MenuItemIngredient>();
        }

        return input
            .Split('\n', System.StringSplitOptions.RemoveEmptyEntries)
            .Select(i => i.Trim())
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => new MenuItemIngredient
            {
                MenuItemId = menuItemId,
                Name = i,
                IsDefault = isDefault
            });
    }
}