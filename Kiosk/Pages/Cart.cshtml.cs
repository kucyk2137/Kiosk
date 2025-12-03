using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using Kiosk.Extensions;
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

        public List<OrderItem> Cart { get; set; } = new();
        [BindProperty]
        public string PaymentMethod { get; set; }
        public decimal Total => Cart.Sum(x => x.MenuItem?.Price * x.Quantity ?? 0);
        public int OrderId { get; set; }

        public void OnGet()
        {
            var sessionCart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();

            Cart = sessionCart
                .Select(x =>
                {
                    x.MenuItem = _context.MenuItems.FirstOrDefault(m => m.Id == x.MenuItemId);
                    return x;
                })
                .Where(x => x.MenuItem != null)
                .ToList();
        }

        public IActionResult OnPost()
        {
            Cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();

            if (Cart.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Koszyk jest pusty");
                return Page();
            }

            var order = new Order
            {
                PaymentMethod = PaymentMethod,
                Items = Cart
                    .Where(x => x.MenuItem != null)
                    .Select(x => new OrderItem
                    {
                        MenuItemId = x.MenuItemId,
                        Quantity = x.Quantity
                    }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            OrderId = order.Id;
            HttpContext.Session.SetObjectAsJson("Cart", new List<OrderItem>());

            return Page();
        }
    }
}
