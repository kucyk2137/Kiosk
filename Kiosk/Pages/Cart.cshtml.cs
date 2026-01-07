using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using Kiosk.Extensions;
using Kiosk.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Kiosk.Pages
{
    public class CartModel : PageModel
    {
        private readonly KioskDbContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public CartModel(KioskDbContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public List<CartItemViewModel> CartItems { get; set; } = new();
        public decimal TotalPrice { get; private set; }

        public void OnGet()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();
            var culture = CultureInfo.CurrentUICulture;

            var menuItemIds = cart.Select(ci => ci.MenuItemId).ToList();
            var menuItems = _context.MenuItems.Where(m => menuItemIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m);
            var ingredientLookup = _context.MenuItemIngredients
                .Where(i => menuItemIds.Contains(i.MenuItemId))
                .GroupBy(i => i.MenuItemId)
                .ToDictionary(g => g.Key, g => g.ToDictionary(i => i.GetDisplayName(culture), i => i.AdditionalPrice));

            CartItems = cart
                .Where(ci => menuItems.ContainsKey(ci.MenuItemId))
                .Select(ci =>
                {
                    var item = menuItems[ci.MenuItemId];
                    var ingredientsJson = ci.SelectedIngredients ?? "[]";
                    var unitPrice = CalculateUnitPrice(item, ingredientsJson, ingredientLookup);

                    return new CartItemViewModel
                    {
                        Item = item,
                        Quantity = ci.Quantity,
                        Ingredients = ingredientsJson,
                        UnitPrice = unitPrice
                    };
                })
                .ToList();

            TotalPrice = CartItems.Sum(ci => ci.UnitPrice * ci.Quantity);
        }

        public IActionResult OnPostRemove(int menuItemId, string selectedIngredients)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();
            selectedIngredients = string.IsNullOrWhiteSpace(selectedIngredients) ? "[]" : selectedIngredients;

            var itemToRemove = cart.FirstOrDefault(ci =>
                ci.MenuItemId == menuItemId &&
                (ci.SelectedIngredients ?? "[]") == selectedIngredients);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostCheckout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();

            if (!cart.Any())
            {
                ModelState.AddModelError(string.Empty, _localizer["Koszyk jest pusty."]);
                OnGet();
                return Page();
            }

            return RedirectToPage("/Payment");
        }
        // składniki dla popupu edycji
        public IActionResult OnGetIngredients(int menuItemId)
        {
            var culture = CultureInfo.CurrentUICulture;
            var ingredients = _context.MenuItemIngredients
                .Where(i => i.MenuItemId == menuItemId)
                .ToList();

            if (!ingredients.Any())
            {
                return new JsonResult(new { defaults = new List<string>(), optionals = new List<string>() });
            }

            var defaults = ingredients
                .Where(i => i.IsDefault)
                .Select(i => new { name = i.GetDisplayName(culture), price = i.AdditionalPrice })
                .ToList();
            var optionals = ingredients
                .Where(i => !i.IsDefault)
                .Select(i => new { name = i.GetDisplayName(culture), price = i.AdditionalPrice })
                .ToList();

            return new JsonResult(new { defaults, optionals });
        }

        // zapis zmian z popupu (składniki + ilość)
        public IActionResult OnPostUpdate(int menuItemId, string oldSelectedIngredients, string newSelectedIngredients, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();

            oldSelectedIngredients = string.IsNullOrWhiteSpace(oldSelectedIngredients) ? "[]" : oldSelectedIngredients;
            newSelectedIngredients = string.IsNullOrWhiteSpace(newSelectedIngredients) ? "[]" : newSelectedIngredients;

            var item = cart.FirstOrDefault(ci =>
                ci.MenuItemId == menuItemId &&
                (ci.SelectedIngredients ?? "[]") == oldSelectedIngredients);

            if (item != null)
            {
                item.SelectedIngredients = newSelectedIngredients;
                item.Quantity = quantity < 1 ? 1 : quantity;

                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            return RedirectToPage();
        }
        private decimal CalculateUnitPrice(MenuItem item, string selectedIngredients, Dictionary<int, Dictionary<string, decimal>> ingredientLookup)
        {
            var basePrice = item.Price;
            if (!ingredientLookup.TryGetValue(item.Id, out var ingredientPrices))
            {
                return basePrice;
            }

            var ingredientList = JsonSerializer.Deserialize<List<string>>(selectedIngredients ?? "[]") ?? new List<string>();
            var additional = ingredientList.Sum(name => ingredientPrices.TryGetValue(name, out var price) ? price : 0);

            return basePrice + additional;
        }

    }

    public class CartItemViewModel
    {
        public MenuItem Item { get; set; }
        public int Quantity { get; set; }
        public string Ingredients { get; set; }
        public decimal UnitPrice { get; set; }
    }
}

