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

        public SiteSettingsService(KioskDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public SiteSettings GetCachedSettings()
        {
            if (_cache.TryGetValue(CacheKey, out SiteSettings? cachedSettings) && cachedSettings is not null)
            {
                return cachedSettings;
            }

            var settings = _context.SiteSettings.AsNoTracking().FirstOrDefault();

            if (settings is null)
            {
                settings = new SiteSettings();
                _context.SiteSettings.Add(settings);
                _context.SaveChanges();
            }

            _cache.Set(CacheKey, settings);
            return settings;
        }

        public async Task<string?> GetHeaderBackgroundAsync()
        {
            var settings = GetCachedSettings();
            return settings.HeaderBackgroundPath;
        }

        public async Task UpdateHeaderBackgroundAsync(string? newPath)
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();

            if (settings is null)
            {
                settings = new SiteSettings();
                _context.SiteSettings.Add(settings);
            }

            settings.HeaderBackgroundPath = newPath;
            await _context.SaveChangesAsync();
            _cache.Set(CacheKey, settings);
        }

        public async Task UpdateAdminLanguageAsync(string language)
        {
            var settings = await EnsureSettingsAsync();
            settings.AdminLanguage = language;
            await PersistAsync(settings);
        }

        public async Task UpdateKitchenLanguageAsync(string language)
        {
            var settings = await EnsureSettingsAsync();
            settings.KitchenLanguage = language;
            await PersistAsync(settings);
        }

        public async Task UpdateOrderDisplayLanguageAsync(string language)
        {
            var settings = await EnsureSettingsAsync();
            settings.OrderDisplayLanguage = language;
            await PersistAsync(settings);
        }

        private async Task<SiteSettings> EnsureSettingsAsync()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();

            if (settings is null)
            {
                settings = new SiteSettings();
                _context.SiteSettings.Add(settings);
            }

            return settings;
        }

        private async Task PersistAsync(SiteSettings settings)
        {
            await _context.SaveChangesAsync();
            _cache.Set(CacheKey, settings);
        }
    }
}