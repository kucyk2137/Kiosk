using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using Kiosk.Extensions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace Kiosk.Pages
{
    public class CartModel : PageModel
    {
        private readonly KioskDbContext _context;

        public CartModel(KioskDbContext context)
        {
            _context = context;
        }

        public List<(MenuItem Item, int Quantity, string Ingredients)> CartItems { get; set; } = new();
        public decimal TotalPrice { get; private set; }

        public void OnGet()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();

            CartItems = cart
                .Select(ci => (Item: _context.MenuItems.FirstOrDefault(m => m.Id == ci.MenuItemId),
                               Quantity: ci.Quantity,
                               Ingredients: ci.SelectedIngredients ?? "[]"))
                .Where(x => x.Item != null)
                .ToList();

            TotalPrice = CartItems.Sum(ci => ci.Item.Price * ci.Quantity);
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
                ModelState.AddModelError(string.Empty, "Koszyk jest pusty.");
                OnGet();
                return Page();
            }

            return RedirectToPage("/Payment");
        }
        // sk³adniki dla popupu edycji
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

        // zapis zmian z popupu (sk³adniki + iloœæ)
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
    }
}
