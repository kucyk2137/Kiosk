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

        public async Task<string?> GetHeaderBackgroundAsync()
        {
            if (_cache.TryGetValue(CacheKey, out string? cachedPath))
            {
                return string.IsNullOrWhiteSpace(cachedPath) ? null : cachedPath;
            }

            var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();

            if (settings is null)
            {
                settings = new SiteSettings();
                _context.SiteSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            _cache.Set(CacheKey, settings.HeaderBackgroundPath ?? string.Empty);
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
            _cache.Set(CacheKey, newPath ?? string.Empty);
        }
    }
}