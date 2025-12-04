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

        public List<(MenuItem Item, int Quantity)> CartItems { get; set; } = new();
        public decimal TotalPrice { get; private set; }

        public void OnGet()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();

            CartItems = cart
                .Select(ci => (Item: _context.MenuItems.FirstOrDefault(m => m.Id == ci.MenuItemId), Quantity: ci.Quantity))
                .Where(x => x.Item != null)
                .ToList();


            TotalPrice = CartItems.Sum(ci => ci.Item.Price * ci.Quantity);
        }

        public IActionResult OnPostRemove(int menuItemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();
            var itemToRemove = cart.FirstOrDefault(ci => ci.MenuItemId == menuItemId);

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

            var order = new Order
            {
                PaymentMethod = "Card",
                Items = cart
                    .Select(ci => new OrderItem
                    {
                        MenuItemId = ci.MenuItemId,
                        Quantity = ci.Quantity
                    })
                    .ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");
            TempData["OrderMessage"] = "Zamówienie zosta³o z³o¿one.";

            return RedirectToPage();
        }
    }
}
