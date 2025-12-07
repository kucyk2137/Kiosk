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

        public void OnGet(int? categoryId)
        {
            Categories = _context.Categories.ToList();

            // produkty polecane (globalnie)
            RecommendedItems = _context.RecommendedProducts
                .Include(rp => rp.MenuItem)!
                .ThenInclude(m => m.Category)
                .Where(rp => rp.MenuItem != null)
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
                                    MenuItemId = i.MenuItemId
                                })
                                .ToList()
                        })
                        .ToList();
                }
            }
        }

        /// <summary>
        /// Dodawanie produktu do koszyka (zarówno z widoku kategorii, jak i z popupu polecanych).
        /// </summary>
        public IActionResult OnPost(int menuItemId, int categoryId, string selectedIngredients, int quantity)
        {
            // JEŒLI selectedIngredients jest puste (np. z produktu polecanego) ? uzupe³niamy domyœlne sk³adniki
            if (string.IsNullOrWhiteSpace(selectedIngredients) || selectedIngredients == "[]")
            {
                var defaultIngredientNames = _context.MenuItemIngredients
                    .Where(i => i.MenuItemId == menuItemId && i.IsDefault)
                    .Select(i => i.Name)
                    .ToList();

                selectedIngredients = JsonSerializer.Serialize(defaultIngredientNames);
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();

            // Proste dodanie pozycji do koszyka (mo¿na rozbudowaæ o ³¹czenie pozycji)
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
        /// Endpoint AJAX dla popupu wyboru sk³adników – zwraca listê domyœlnych i opcjonalnych.
        /// </summary>
        public IActionResult OnGetIngredients(int menuItemId)
        {
            var product = _context.MenuItems
                .Where(m => m.Id == menuItemId)
                .Select(m => new
                {
                    defaults = m.Ingredients.Where(i => i.IsDefault).Select(i => i.Name).ToList(),
                    optionals = m.Ingredients.Where(i => !i.IsDefault).Select(i => i.Name).ToList()
                })
                .FirstOrDefault();

            if (product == null)
            {
                return new JsonResult(new { defaults = new List<string>(), optionals = new List<string>() });
            }

            return new JsonResult(product);
        }
    }
}
