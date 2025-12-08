using Kiosk.Data;
using Kiosk.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Kiosk.Services
{
    public class SiteSettingsService
    {
        private readonly KioskDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "site-settings";

        public const string DefaultAccentColor = "#ff7a00";
        public const string DefaultBorderColor = "#e6ecf3";
        public const string DefaultShadowColor = "#2d3d65";

        public SiteSettingsService(KioskDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<string?> GetHeaderBackgroundAsync()
        {
            var settings = await GetOrCreateSettingsAsync();
            return settings.HeaderBackgroundPath;
        }

        public async Task UpdateHeaderBackgroundAsync(string? newPath)
        {
            var settings = await GetOrCreateSettingsAsync();

            settings.HeaderBackgroundPath = newPath;
            await _context.SaveChangesAsync();
            _cache.Set(CacheKey, settings);
        }

        public async Task<StyleSettings> GetStyleSettingsAsync()
        {
            var settings = await GetOrCreateSettingsAsync();

            var accent = string.IsNullOrWhiteSpace(settings.PrimaryButtonColor)
                ? DefaultAccentColor
                : settings.PrimaryButtonColor;

            var border = string.IsNullOrWhiteSpace(settings.PanelBorderColor)
                ? DefaultBorderColor
                : settings.PanelBorderColor;

            var shadow = string.IsNullOrWhiteSpace(settings.ShadowColor)
                ? DefaultShadowColor
                : settings.ShadowColor;

            return new StyleSettings(
                accent,
                LightenColor(accent, 0.12),
                LightenColor(accent, 0.68),
                border,
                BuildShadow(shadow, 0.16),
                BuildShadow(accent, 0.32, "0 14px 32px"),
                GetContrastingTextColor(accent),
                settings.PrimaryButtonColor,
                settings.PanelBorderColor,
                settings.ShadowColor);
        }

        public async Task UpdateStyleSettingsAsync(string? primaryColor, string? panelBorderColor, string? shadowColor)
        {
            var settings = await GetOrCreateSettingsAsync();

            settings.PrimaryButtonColor = NormalizeColor(primaryColor);
            settings.PanelBorderColor = NormalizeColor(panelBorderColor);
            settings.ShadowColor = NormalizeColor(shadowColor);

            await _context.SaveChangesAsync();
            _cache.Set(CacheKey, settings);
        }

        public async Task ResetStyleSettingsAsync()
        {
            var settings = await GetOrCreateSettingsAsync();

            settings.PrimaryButtonColor = null;
            settings.PanelBorderColor = null;
            settings.ShadowColor = null;

            await _context.SaveChangesAsync();
            _cache.Set(CacheKey, settings);
        }

        private async Task<SiteSettings> GetOrCreateSettingsAsync()
        {
            if (_cache.TryGetValue(CacheKey, out SiteSettings? cachedSettings) && cachedSettings is not null)
            {
                if (_context.Entry(cachedSettings).State == EntityState.Detached)
                {
                    _context.SiteSettings.Attach(cachedSettings);
                }

                return cachedSettings;
            }

            var settings = await _context.SiteSettings.FirstOrDefaultAsync();

            if (settings is null)
            {
                settings = new SiteSettings();
                _context.SiteSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            _cache.Set(CacheKey, settings);
            return settings;
        }

        private static string BuildShadow(string hexColor, double opacity, string offsets = "0 24px 45px")
        {
            var rgba = ToRgba(hexColor, opacity);
            return $"{offsets} {rgba}";
        }

        private static string ToRgba(string hex, double alpha)
        {
            if (!hex.StartsWith('#') || (hex.Length != 7 && hex.Length != 4))
            {
                return hex;
            }

            var expandedHex = hex.Length == 4 ? $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}" : hex;

            var r = Convert.ToInt32(expandedHex.Substring(1, 2), 16);
            var g = Convert.ToInt32(expandedHex.Substring(3, 2), 16);
            var b = Convert.ToInt32(expandedHex.Substring(5, 2), 16);

            return $"rgba({r}, {g}, {b}, {alpha.ToString(System.Globalization.CultureInfo.InvariantCulture)})";
        }

        private static string LightenColor(string hex, double factor)
        {
            if (!hex.StartsWith('#') || (hex.Length != 7 && hex.Length != 4))
            {
                return hex;
            }

            var expandedHex = hex.Length == 4 ? $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}" : hex;

            var r = Convert.ToInt32(expandedHex.Substring(1, 2), 16);
            var g = Convert.ToInt32(expandedHex.Substring(3, 2), 16);
            var b = Convert.ToInt32(expandedHex.Substring(5, 2), 16);

            r = (int)Math.Min(255, r + 255 * factor);
            g = (int)Math.Min(255, g + 255 * factor);
            b = (int)Math.Min(255, b + 255 * factor);

            return $"#{r:X2}{g:X2}{b:X2}";
        }

        private static string GetContrastingTextColor(string hex)
        {
            if (!hex.StartsWith('#') || (hex.Length != 7 && hex.Length != 4))
            {
                return "#ffffff";
            }

            var expandedHex = hex.Length == 4 ? $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}" : hex;

            var r = Convert.ToInt32(expandedHex.Substring(1, 2), 16);
            var g = Convert.ToInt32(expandedHex.Substring(3, 2), 16);
            var b = Convert.ToInt32(expandedHex.Substring(5, 2), 16);

            // Perceived brightness per ITU-R BT.601
            var brightness = (0.299 * r) + (0.587 * g) + (0.114 * b);

            return brightness > 186 ? "#0f0c04" : "#ffffff";
        }

        private static string? NormalizeColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return null;
            }

            return color.Trim();
        }
    }

    public record StyleSettings(
        string AccentColor,
        string AccentStrongColor,
        string AccentSoftColor,
        string PanelBorderColor,
        string PanelShadow,
        string ButtonShadow,
        string AccentTextColor,
        string? SavedAccentColor,
        string? SavedPanelBorderColor,
        string? SavedShadowColor);
}