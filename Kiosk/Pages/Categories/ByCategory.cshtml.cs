using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Extensions;
using Kiosk.Models;
using System.Collections.Generic;
using System.Linq;

namespace Kiosk.Pages.Categories
{
    public class ByCategoryModel : PageModel
    {
        private readonly KioskDbContext _context;

        public ByCategoryModel(KioskDbContext context)
        {
            _context = context;
        }

        public Category Category { get; set; }
        public List<MenuItem> Products { get; set; } = new();

        public IActionResult OnGet(int categoryId)
        {
            Category = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (Category == null)
                return RedirectToPage("/Menu");

            Products = _context.MenuItems
                               .Where(m => m.CategoryId == categoryId)
                               .Select(m => new MenuItem
                               {
                                   Id = m.Id,
                                   Name = m.Name,
                                   Description = m.Description,
                                   Price = m.Price,
                                   ImageUrl = m.ImageUrl,
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

            return Page();
        }

        public IActionResult OnPost(int menuItemId, int categoryId, string selectedIngredients, int quantity)
        {
            var product = _context.MenuItems.FirstOrDefault(m => m.Id == menuItemId && m.CategoryId == categoryId);
            if (product == null)
            {
                return RedirectToPage("/Menu");
            }

            selectedIngredients = string.IsNullOrWhiteSpace(selectedIngredients) ? "[]" : selectedIngredients;
            if (quantity < 1) quantity = 1;

            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();

            var existingItem = cart.FirstOrDefault(ci =>
                ci.MenuItemId == menuItemId &&
                (ci.SelectedIngredients ?? "[]") == selectedIngredients);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new OrderItem
                {
                    MenuItemId = menuItemId,
                    Quantity = quantity,
                    SelectedIngredients = selectedIngredients
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToPage(new { categoryId });
        }

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
