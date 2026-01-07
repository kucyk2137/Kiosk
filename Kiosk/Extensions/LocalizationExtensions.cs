using System;
using System.Globalization;
using Kiosk.Models;

namespace Kiosk.Extensions
{
    public static class LocalizationExtensions
    {
        public static string GetDisplayName(this MenuItem item, CultureInfo culture)
        {
            if (item == null)
            {
                return string.Empty;
            }

            return IsEnglish(culture) && !string.IsNullOrWhiteSpace(item.NameEn)
                ? item.NameEn
                : item.Name;
        }

        public static string GetDisplayDescription(this MenuItem item, CultureInfo culture)
        {
            if (item == null)
            {
                return string.Empty;
            }

            return IsEnglish(culture) && !string.IsNullOrWhiteSpace(item.DescriptionEn)
                ? item.DescriptionEn
                : item.Description;
        }

        public static string GetDisplayName(this Category category, CultureInfo culture)
        {
            if (category == null)
            {
                return string.Empty;
            }

            return IsEnglish(culture) && !string.IsNullOrWhiteSpace(category.NameEn)
                ? category.NameEn
                : category.Name;
        }

        public static string GetDisplayName(this MenuItemIngredient ingredient, CultureInfo culture)
        {
            if (ingredient == null)
            {
                return string.Empty;
            }

            return IsEnglish(culture) && !string.IsNullOrWhiteSpace(ingredient.NameEn)
                ? ingredient.NameEn
                : ingredient.Name;
        }

        public static bool IsSetsCategory(this Category category)
        {
            if (category == null)
            {
                return false;
            }

            return string.Equals(category.Name, "Zestawy", StringComparison.OrdinalIgnoreCase)
                || string.Equals(category.NameEn, "Sets", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEnglish(CultureInfo culture)
        {
            return string.Equals(culture.TwoLetterISOLanguageName, "en", StringComparison.OrdinalIgnoreCase);
        }
    }
}
