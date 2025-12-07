using System.Linq;
using Kiosk.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kiosk.Pages
{
    public class LockScreenModel : PageModel
    {
        private readonly KioskDbContext _context;

        public LockScreenModel(KioskDbContext context)
        {
            _context = context;
        }

        public string? BackgroundImagePath { get; private set; }

        public void OnGet()
        {
            BackgroundImagePath = _context.LockScreenBackgrounds
                .OrderByDescending(b => b.UploadedAt)
                .Select(b => b.ImagePath)
                .FirstOrDefault();
        }
    }
}