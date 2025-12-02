using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using Kiosk.Extensions;

namespace Kiosk.Pages.Categories
{
    public class SetsModel : PageModel
    {
        private readonly KioskDbContext _context;
        public SetsModel(KioskDbContext context) => _context = context;

        public List<MenuItem> Products { get; set; } = new();

        public void OnGet()
        {
            Products = _context.MenuItems
                .Where(m => m.Category == "Sets")
                .ToList();
        }

        public IActionResult OnPostAddToCart(int MenuItemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();
            var existingItem = cart.FirstOrDefault(x => x.MenuItemId == MenuItemId);
            if (existingItem != null) existingItem.Quantity++;
            else cart.Add(new OrderItem { MenuItemId = MenuItemId, Quantity = 1 });
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToPage();
        }
    }
}
