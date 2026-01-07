using System;
using System.IO;
using System.Linq;
using Kiosk.Data;
using Kiosk.Models;
using Kiosk.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Kiosk.Pages.Admin
{
    public class LockScreenBackgroundModel : PageModel
    {
        private readonly KioskDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public LockScreenBackgroundModel(KioskDbContext context, IWebHostEnvironment environment, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _environment = environment;
            _localizer = localizer;
        }

        [BindProperty]
        public IFormFile ImageFile { get; set; }

        public LockScreenBackground? CurrentBackground { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
            CurrentBackground = _context.LockScreenBackgrounds
                .OrderByDescending(b => b.UploadedAt)
                .FirstOrDefault();
        }

        public IActionResult OnPost()
        {
            if (ImageFile == null || ImageFile.Length == 0)
            {
                ModelState.AddModelError(nameof(ImageFile), _localizer["Plik obrazu jest wymagany."]);
            }

            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "lockscreen");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                ImageFile.CopyTo(stream);
            }

            var existingBackgrounds = _context.LockScreenBackgrounds.ToList();
            foreach (var oldBackground in existingBackgrounds)
            {
                if (!string.IsNullOrWhiteSpace(oldBackground.ImagePath))
                {
                    var oldPhysicalPath = Path.Combine(_environment.WebRootPath, oldBackground.ImagePath.TrimStart('/')
                        .Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(oldPhysicalPath))
                    {
                        System.IO.File.Delete(oldPhysicalPath);
                    }
                }
            }

            if (existingBackgrounds.Any())
            {
                _context.LockScreenBackgrounds.RemoveRange(existingBackgrounds);
            }

            var background = new LockScreenBackground
            {
                ImagePath = $"/images/lockscreen/{fileName}",
                UploadedAt = DateTime.UtcNow
            };

            _context.LockScreenBackgrounds.Add(background);
            _context.SaveChanges();

            Message = "Nowe tło zostało zapisane.";
            ImageFile = null;
            CurrentBackground = background;
            return Page();
        }
    }
}