using Kiosk.Data;
using Kiosk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

public class AddProductModel : PageModel
{
    private readonly KioskDbContext _context;
    public AddProductModel(KioskDbContext context) => _context = context;

    [BindProperty]
    public MenuItem MenuItem { get; set; }

    [BindProperty]
    public string DefaultIngredientsInput { get; set; }

    [BindProperty]
    public string OptionalIngredientsInput { get; set; }

    public SelectList CategorySelectList { get; set; }
    public string Message { get; set; }

    public void OnGet()
    {
        CategorySelectList = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");
    }

    public IActionResult OnPost()
    {
        CategorySelectList = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");

        if (!ModelState.IsValid)
        {
            var allErrors = ModelState
                .SelectMany(kvp => kvp.Value.Errors.Select(e => $"{kvp.Key}: {e.ErrorMessage}"))
                .ToList();

            Message = "Formularz zawiera b³êdy: " + string.Join(" | ", allErrors);
            return Page();
        }

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