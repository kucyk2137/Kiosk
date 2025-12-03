using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Models;
using Kiosk.Data;
using Kiosk.Extensions;

namespace Kiosk.Pages
{
    public class MenuModel : PageModel
    {
        private readonly KioskDbContext _context;

        public MenuModel(KioskDbContext context)
        {
            _context = context;
        }

        public List<MenuItem> MenuItems { get; set; } = new();

        public void OnGet()
        {
            MenuItems = _context.MenuItems.ToList();
        }

        public IActionResult OnPost(int MenuItemId)
        {
            // Pobranie koszyka z sesji
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();

            var existingItem = cart.FirstOrDefault(x => x.MenuItemId == MenuItemId);
            if (existingItem != null)
                existingItem.Quantity++;
            else
                cart.Add(new OrderItem { MenuItemId = MenuItemId, Quantity = 1 });

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToPage(); // przekierowanie do tej samej strony
        }
    }
}
