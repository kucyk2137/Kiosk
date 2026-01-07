using System.Linq;
using Kiosk.Data;
using Kiosk.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Kiosk.Pages
{
    public class OrderTypeModel : PageModel
    {
        private readonly KioskDbContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public OrderTypeModel(KioskDbContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public string? BackgroundImagePath { get; private set; }

        public void OnGet()
        {
            LoadBackgroundImage();
        }

        public IActionResult OnPost(string orderType)
        {
            LoadBackgroundImage();

            if (string.IsNullOrWhiteSpace(orderType))
            {
                ModelState.AddModelError(string.Empty, "Wybierz rodzaj zamówienia.");
                return Page();
            }

            HttpContext.Session.SetString("OrderType", orderType);
            return RedirectToPage("/Menu");
        }

        private void LoadBackgroundImage()
        {
            BackgroundImagePath = _context.LockScreenBackgrounds
                .OrderByDescending(b => b.UploadedAt)
                .Select(b => b.ImagePath)
                .FirstOrDefault();
        }
    }
}