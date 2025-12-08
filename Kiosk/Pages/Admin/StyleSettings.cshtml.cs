using Kiosk.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kiosk.Pages.Admin
{
    public class StyleSettingsModel : PageModel
    {
        private readonly SiteSettingsService _siteSettingsService;

        public StyleSettingsModel(SiteSettingsService siteSettingsService)
        {
            _siteSettingsService = siteSettingsService;
        }

        [BindProperty]
        public string AccentColor { get; set; } = SiteSettingsService.DefaultAccentColor;

        [BindProperty]
        public string PanelBorderColor { get; set; } = SiteSettingsService.DefaultBorderColor;

        [BindProperty]
        public string ShadowColor { get; set; } = SiteSettingsService.DefaultShadowColor;

        public StyleSettings? CurrentStyle { get; set; }

        public async Task OnGetAsync()
        {
            CurrentStyle = await _siteSettingsService.GetStyleSettingsAsync();

            AccentColor = CurrentStyle.SavedAccentColor ?? CurrentStyle.AccentColor;
            PanelBorderColor = CurrentStyle.SavedPanelBorderColor ?? CurrentStyle.PanelBorderColor;
            ShadowColor = CurrentStyle.SavedShadowColor ?? SiteSettingsService.DefaultShadowColor;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _siteSettingsService.UpdateStyleSettingsAsync(AccentColor, PanelBorderColor, ShadowColor);
            TempData["StatusMessage"] = "Styl przycisków i ramek został zapisany.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetAsync()
        {
            await _siteSettingsService.ResetStyleSettingsAsync();
            TempData["StatusMessage"] = "Przywrócono domyślne style.";
            return RedirectToPage();
        }
    }
}
