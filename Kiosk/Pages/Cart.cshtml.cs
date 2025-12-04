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

        public void OnGet()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();

            CartItems = cart
                .Select(ci => (Item: _context.MenuItems.FirstOrDefault(m => m.Id == ci.MenuItemId), Quantity: ci.Quantity))
                .Where(x => x.Item != null)
                .ToList();
        }
    }
}
