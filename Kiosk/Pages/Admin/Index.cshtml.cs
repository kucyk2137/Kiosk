using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Kiosk.Data;
using Kiosk.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace Kiosk.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly KioskDbContext _context;

        public IndexModel(KioskDbContext context)
        {
            _context = context;
        }

        public List<MenuItem> MenuItems { get; set; } = new();
        public List<MenuItem> RecommendedMenuItems { get; set; } = new();
        public List<MenuItem> AvailableForRecommendation { get; set; } = new();
        public void OnGet()
        {

            MenuItems = _context.MenuItems
                  .Include(m => m.Category)
                  .ToList();

            RecommendedMenuItems = _context.RecommendedProducts
                .Include(rp => rp.MenuItem)!
                .ThenInclude(m => m.Category)
                .Where(rp => rp.MenuItem != null)
                .Select(rp => rp.MenuItem!)
                .ToList();

            var recommendedIds = RecommendedMenuItems.Select(m => m.Id).ToHashSet();
            AvailableForRecommendation = _context.MenuItems
                .Where(m => !recommendedIds.Contains(m.Id))
                .OrderBy(m => m.Name)
                .ToList();
        }

        public IActionResult OnPostDelete(int MenuItemId)
        {
            var item = _context.MenuItems.FirstOrDefault(m => m.Id == MenuItemId);
            if (item != null)
            {
                _context.MenuItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToPage();
        }
        public IActionResult OnPostAddRecommended(int menuItemId)
        {
            if (!_context.RecommendedProducts.Any(rp => rp.MenuItemId == menuItemId))
            {
                _context.RecommendedProducts.Add(new RecommendedProduct { MenuItemId = menuItemId });
                _context.SaveChanges();
            }

            return RedirectToPage();
        }

        public IActionResult OnPostRemoveRecommended(int menuItemId)
        {
            var recommended = _context.RecommendedProducts.FirstOrDefault(rp => rp.MenuItemId == menuItemId);
            if (recommended != null)
            {
                _context.RecommendedProducts.Remove(recommended);
                _context.SaveChanges();
            }

            return RedirectToPage();
        }
    }
}
