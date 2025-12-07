using Kiosk.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;

namespace Kiosk.Pages.Admin
{
    public class HeaderBackground : PageModel
    {
        private readonly SiteSettingsService _siteSettingsService;
        private readonly IWebHostEnvironment _environment;

        public HeaderBackground(SiteSettingsService siteSettingsService, IWebHostEnvironment environment)
        {
            _siteSettingsService = siteSettingsService;
            _environment = environment;
        }

        [BindProperty]
        public IFormFile? HeaderImage { get; set; }

        public string? CurrentHeaderImage { get; set; }

        public async Task OnGetAsync()
        {
            CurrentHeaderImage = await _siteSettingsService.GetHeaderBackgroundAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CurrentHeaderImage = await _siteSettingsService.GetHeaderBackgroundAsync();

            if (HeaderImage is null || HeaderImage.Length == 0)
            {
                ModelState.AddModelError(nameof(HeaderImage), "Proszê wybraæ plik graficzny.");
                return Page();
            }

            var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads", "headers");
            Directory.CreateDirectory(uploadsDirectory);

            var extension = Path.GetExtension(HeaderImage.FileName);
            var fileName = $"header-{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
            var physicalPath = Path.Combine(uploadsDirectory, fileName);

            using (var stream = System.IO.File.Create(physicalPath))
            {
                await HeaderImage.CopyToAsync(stream);
            }

            if (!string.IsNullOrWhiteSpace(CurrentHeaderImage))
            {
                var currentPhysicalPath = Path.Combine(_environment.WebRootPath, CurrentHeaderImage.TrimStart('/')
                    .Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(currentPhysicalPath))
                {
                    System.IO.File.Delete(currentPhysicalPath);
                }
            }

            var relativePath = $"/uploads/headers/{fileName}";
            await _siteSettingsService.UpdateHeaderBackgroundAsync(relativePath);
            TempData["StatusMessage"] = "Zdjêcie nag³ówka zosta³o zaktualizowane.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAsync()
        {
            CurrentHeaderImage = await _siteSettingsService.GetHeaderBackgroundAsync();

            if (!string.IsNullOrWhiteSpace(CurrentHeaderImage))
            {
                var currentPhysicalPath = Path.Combine(_environment.WebRootPath, CurrentHeaderImage.TrimStart('/')
                    .Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(currentPhysicalPath))
                {
                    System.IO.File.Delete(currentPhysicalPath);
                }

                await _siteSettingsService.UpdateHeaderBackgroundAsync(null);
            }

            TempData["StatusMessage"] = "Zdjêcie nag³ówka zosta³o usuniête.";
            return RedirectToPage();
        }
    }
}