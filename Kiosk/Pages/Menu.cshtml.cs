using Kiosk.Data;
using Kiosk.Extensions; // dla GetObjectFromJson / SetObjectAsJson
using Kiosk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Kiosk.Pages
{
    public class MenuModel : PageModel
    {
        private readonly KioskDbContext _context;

        public MenuModel(KioskDbContext context)
        {
            _context = context;
        }

        public List<Category> Categories { get; set; } = new();
        public Category? SelectedCategory { get; set; }
        public List<MenuItem> Products { get; set; } = new();
        public List<MenuItem> RecommendedItems { get; set; } = new();
        public Dictionary<int, List<string>> SetContents { get; set; } = new();
        public HashSet<int> ProductsWithSets { get; set; } = new();
        public void OnGet(int? categoryId)
        {
            Categories = _context.Categories.ToList();
            var culture = CultureInfo.CurrentUICulture;
            ProductsWithSets = _context.ProductSetItems
               .Select(p => p.MenuItemId)
               .ToHashSet();
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();
            var cartMenuItemIds = cart.Select(c => c.MenuItemId).ToHashSet();
            // produkty polecane (globalnie)
            RecommendedItems = _context.RecommendedProducts
                .Include(rp => rp.MenuItem)!
                .ThenInclude(m => m.Category)
                .Where(rp => rp.MenuItem != null && !cartMenuItemIds.Contains(rp.MenuItem.Id))
                .Select(rp => rp.MenuItem!)
                .ToList();

            if (categoryId.HasValue)
            {
                SelectedCategory = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
                if (SelectedCategory != null)
                {
                    Products = _context.MenuItems
                        .Where(m => m.CategoryId == categoryId)
                        .Select(m => new MenuItem
                        {
                            Id = m.Id,
                            Name = m.Name,
                            NameEn = m.NameEn,
                            Description = m.Description,
                            DescriptionEn = m.DescriptionEn,
                            Price = m.Price,
                            Image = m.Image,
                            CategoryId = m.CategoryId,
                            Ingredients = m.Ingredients
                                .Select(i => new MenuItemIngredient
                                {
                                    Id = i.Id,
                                    Name = i.Name,
                                    NameEn = i.NameEn,
                                    IsDefault = i.IsDefault,
                                    MenuItemId = i.MenuItemId,
                                    AdditionalPrice = i.AdditionalPrice
                                })
                                .ToList()
                        })
                        .ToList();
                    var displayedIds = Products.Select(p => p.Id).ToList();
                    SetContents = _context.ProductSets
                        .Include(ps => ps.Items)
                        .ThenInclude(i => i.MenuItem)
                        .Where(ps => displayedIds.Contains(ps.SetMenuItemId))
                        .AsEnumerable()
                        .ToDictionary(ps => ps.SetMenuItemId, ps => ps.Items.Select(i => i.MenuItem.GetDisplayName(culture)).ToList());
                }
            }
        }

        /// <summary>
        /// Dodawanie produktu do koszyka (zarówno z widoku kategorii, jak i z popupu polecanych).
        /// </summary>
        public IActionResult OnPost(int menuItemId, int categoryId, string selectedIngredients, int quantity)
        {
            var culture = CultureInfo.CurrentUICulture;
            // JEŚLI selectedIngredients jest puste (np. z produktu polecanego) ? uzupełniamy domyślne składniki
            if (string.IsNullOrWhiteSpace(selectedIngredients) || selectedIngredients == "[]")
            {
                var defaultIngredientNames = _context.MenuItemIngredients
                    .Where(i => i.MenuItemId == menuItemId && i.IsDefault)
                    .Select(i => i.GetDisplayName(culture))
                    .ToList();

                selectedIngredients = JsonSerializer.Serialize(defaultIngredientNames);
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();

            // Proste dodanie pozycji do koszyka (można rozbudować o łączenie pozycji)
            cart.Add(new OrderItem
            {
                MenuItemId = menuItemId,
                Quantity = quantity,
                SelectedIngredients = selectedIngredients
            });

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToPage(new { categoryId });
        }

        /// <summary>
        /// Endpoint AJAX dla popupu wyboru składników – zwraca listę domyślnych i opcjonalnych.
        /// </summary>
        public IActionResult OnGetIngredients(int menuItemId)
        {
            var culture = CultureInfo.CurrentUICulture;
            // 1. Sprawdź, czy to jest zestaw (SetMenuItem)
            var productSet = _context.ProductSets
                .Include(ps => ps.Items)
                    .ThenInclude(psi => psi.MenuItem)
                        .ThenInclude(mi => mi.Ingredients)
                .FirstOrDefault(ps => ps.SetMenuItemId == menuItemId);

            if (productSet != null)
            {
                // Zbierz składniki ze wszystkich produktów w zestawie
                var defaultList = new List<object>();
                var optionalList = new List<object>();

                foreach (var item in productSet.Items)
                {
                    var product = item.MenuItem;
                    if (product == null) continue;

                    var productName = product.GetDisplayName(culture);

                    var defaults = product.Ingredients
                        .Where(i => i.IsDefault)
                        .Select(i => new
                        {
                            name = $"{productName}: {i.GetDisplayName(culture)}",
                            price = i.AdditionalPrice
                        });

                    var optionals = product.Ingredients
                        .Where(i => !i.IsDefault)
                        .Select(i => new
                        {
                            name = $"{productName}: {i.GetDisplayName(culture)}",
                            price = i.AdditionalPrice
                        });

                    defaultList.AddRange(defaults);
                    optionalList.AddRange(optionals);
                }

                return new JsonResult(new
                {
                    defaults = defaultList,
                    optionals = optionalList
                });
            }

            // 2. Zwykły produkt – dotychczasowe zachowanie
            var ingredients = _context.MenuItemIngredients
                .Where(i => i.MenuItemId == menuItemId)
                .ToList();

            if (!ingredients.Any())
            {
                return new JsonResult(new { defaults = new List<object>(), optionals = new List<object>() });
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
        public IActionResult OnGetSetsForProduct(int menuItemId)
        {
            var culture = CultureInfo.CurrentUICulture;
            var sets = _context.ProductSets
                .Include(ps => ps.SetMenuItem)!
                .ThenInclude(mi => mi.Category)
                .Include(ps => ps.Items)
                .ThenInclude(i => i.MenuItem)
                .Where(ps => ps.SetMenuItem != null && ps.Items.Any(i => i.MenuItemId == menuItemId))
                .Select(ps => new
                {
                    id = ps.SetMenuItemId,
                    name = ps.SetMenuItem!.GetDisplayName(culture),
                    price = ps.SetMenuItem.Price,
                    description = ps.SetMenuItem.GetDisplayDescription(culture),
                    image = ps.SetMenuItem.Image,
                    categoryId = ps.SetMenuItem.CategoryId,
                    items = ps.Items.Select(i => new { id = i.MenuItemId, name = i.MenuItem.GetDisplayName(culture), image = i.MenuItem.Image }).ToList()
                })
                .ToList();

            return new JsonResult(sets);
        }
    }
}
