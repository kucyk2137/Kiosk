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
                               .ToList();

            return Page();
        }

        public IActionResult OnPost(int menuItemId, int categoryId)
        {
            var product = _context.MenuItems.FirstOrDefault(m => m.Id == menuItemId && m.CategoryId == categoryId);
            if (product == null)
            {
                return RedirectToPage("/Menu");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();
            var existingItem = cart.FirstOrDefault(ci => ci.MenuItemId == menuItemId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new OrderItem
                {
                    MenuItemId = menuItemId,
                    Quantity = 1
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToPage(new { categoryId });
        }
    }
}
