using Kiosk.Data;
using Kiosk.Extensions; // dla GetObjectFromJson / SetObjectAsJson
using Kiosk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
                            Description = m.Description,
                            Price = m.Price,
                            Image = m.Image,
                            CategoryId = m.CategoryId,
                            Ingredients = m.Ingredients
                                .Select(i => new MenuItemIngredient
                                {
                                    Id = i.Id,
                                    Name = i.Name,
                                    IsDefault = i.IsDefault,
                                    MenuItemId = i.MenuItemId,
                                    AdditionalPrice = i.AdditionalPrice,
                                    Quantity = i.Quantity
                                })
                                .ToList()
                        })
                        .ToList();
                    var displayedIds = Products.Select(p => p.Id).ToList();
                    SetContents = _context.ProductSets
                        .Include(ps => ps.Items)
                        .ThenInclude(i => i.MenuItem)
                        .Where(ps => displayedIds.Contains(ps.SetMenuItemId))
                        .ToDictionary(ps => ps.SetMenuItemId, ps => ps.Items.Select(i => i.MenuItem.Name).ToList());
                }
            }
        }


        public IActionResult OnPost(int menuItemId, int categoryId, string selectedIngredients, int quantity)
        {

            if (string.IsNullOrWhiteSpace(selectedIngredients) || selectedIngredients == "[]")
            {
                selectedIngredients = string.IsNullOrWhiteSpace(selectedIngredients) ? "[]" : selectedIngredients;
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();


            cart.Add(new OrderItem
            {
                MenuItemId = menuItemId,
                Quantity = quantity,
                SelectedIngredients = selectedIngredients
            });

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToPage(new { categoryId });
        }


        public IActionResult OnGetIngredients(int menuItemId)
        {
        
            var productSet = _context.ProductSets
                .Include(ps => ps.Items)
                    .ThenInclude(psi => psi.MenuItem)
                        .ThenInclude(mi => mi.Ingredients)
                .FirstOrDefault(ps => ps.SetMenuItemId == menuItemId);

            if (productSet != null)
            {
                var defaultList = new List<object>();
                var optionalList = new List<object>();

                foreach (var item in productSet.Items)
                {
                    var product = item.MenuItem;
                    if (product == null) continue;

                    var productName = product.Name;

                    var defaults = product.Ingredients
                        .Where(i => i.IsDefault)
                        .Select(i => new
                        {
                            name = $"{productName}: {i.Name}",
                            price = i.AdditionalPrice,
                            quantity = i.Quantity
                        });

                    var optionals = product.Ingredients
                        .Where(i => !i.IsDefault)
                        .Select(i => new
                        {
                            name = $"{productName}: {i.Name}",
                            price = i.AdditionalPrice,
                            quantity = i.Quantity
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


            var singleProduct = _context.MenuItems
                .Where(m => m.Id == menuItemId)
                .Select(m => new
                {
                    defaults = m.Ingredients.Where(i => i.IsDefault)
                        .Select(i => new { name = i.Name, price = i.AdditionalPrice, quantity = i.Quantity }).ToList(),
                    optionals = m.Ingredients.Where(i => !i.IsDefault)
                        .Select(i => new { name = i.Name, price = i.AdditionalPrice, quantity = i.Quantity }).ToList()
                })
                .FirstOrDefault();

            if (singleProduct == null)
            {
                return new JsonResult(new { defaults = new List<object>(), optionals = new List<object>() });
            }

            return new JsonResult(singleProduct);
        }
        public IActionResult OnGetSetsForProduct(int menuItemId)
        {
            var sets = _context.ProductSets
                .Include(ps => ps.SetMenuItem)!
                .ThenInclude(mi => mi.Category)
                .Include(ps => ps.Items)
                .ThenInclude(i => i.MenuItem)
                .Where(ps => ps.SetMenuItem != null && ps.Items.Any(i => i.MenuItemId == menuItemId))
                .Select(ps => new
                {
                    id = ps.SetMenuItemId,
                    name = ps.SetMenuItem!.Name,
                    price = ps.SetMenuItem.Price,
                    description = ps.SetMenuItem.Description,
                    image = ps.SetMenuItem.Image,
                    categoryId = ps.SetMenuItem.CategoryId,
                    items = ps.Items.Select(i => new { id = i.MenuItemId, name = i.MenuItem.Name, image = i.MenuItem.Image }).ToList()
                })
                .ToList();

            return new JsonResult(sets);
        }
    }
}
