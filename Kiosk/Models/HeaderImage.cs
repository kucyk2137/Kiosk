namespace Kiosk.Models
{
    public class SiteSettings
    {
        public int Id { get; set; }

        public string? HeaderBackgroundPath { get; set; }

        public string? AdminLanguage { get; set; }

        public string? KitchenLanguage { get; set; }

        public string? OrderDisplayLanguage { get; set; }
    }
}