using Kiosk.Models;
using Microsoft.AspNetCore.Http;

namespace Kiosk.Services
{
    public enum LanguageArea
    {
        Ordering,
        Admin,
        Kitchen,
        OrderDisplay
    }

    public class LanguageService
    {
        private const string OrderingSessionKey = "ordering-language";
        private static readonly HashSet<string> SupportedLanguages = new(new[] { "pl", "en", "de" }, StringComparer.OrdinalIgnoreCase);

        private readonly SiteSettingsService _siteSettingsService;

        public LanguageService(SiteSettingsService siteSettingsService)
        {
            _siteSettingsService = siteSettingsService;
        }

        public string GetLanguage(HttpContext httpContext, LanguageArea area)
        {
            return area switch
            {
                LanguageArea.Ordering => httpContext.Session.GetString(OrderingSessionKey) ?? "pl",
                LanguageArea.Admin => _siteSettingsService.GetCachedSettings().AdminLanguage ?? "pl",
                LanguageArea.Kitchen => _siteSettingsService.GetCachedSettings().KitchenLanguage ?? "pl",
                LanguageArea.OrderDisplay => _siteSettingsService.GetCachedSettings().OrderDisplayLanguage ?? "pl",
                _ => "pl"
            };
        }

        public void SetOrderingLanguage(HttpContext httpContext, string language)
        {
            var normalized = Normalize(language);
            httpContext.Session.SetString(OrderingSessionKey, normalized);
        }

        public async Task UpdatePersistentLanguageAsync(LanguageArea area, string language)
        {
            var normalized = Normalize(language);
            switch (area)
            {
                case LanguageArea.Admin:
                    await _siteSettingsService.UpdateAdminLanguageAsync(normalized);
                    break;
                case LanguageArea.Kitchen:
                    await _siteSettingsService.UpdateKitchenLanguageAsync(normalized);
                    break;
                case LanguageArea.OrderDisplay:
                    await _siteSettingsService.UpdateOrderDisplayLanguageAsync(normalized);
                    break;
            }
        }

        public string Translate(HttpContext httpContext, LanguageArea area, string key, string fallback)
        {
            var lang = GetLanguage(httpContext, area);

            if (LanguageResources.TryGetTranslation(lang, key, out var translation))
            {
                return translation;
            }

            return fallback;
        }

        private static string Normalize(string language)
        {
            var normalized = language?.ToLowerInvariant() ?? "pl";
            return SupportedLanguages.Contains(normalized) ? normalized : "pl";
        }
    }

    internal static class LanguageResources
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Translations = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["orderType.title"] = "Choose order type",
                ["orderType.subtitle"] = "Let us know if you are eating in or taking away.",
                ["orderType.eatIn"] = "Dine in",
                ["orderType.takeaway"] = "Take away",
                ["menu.cart"] = "Cart",
                ["menu.addToCart"] = "Add to cart",
                ["menu.chooseCategory"] = "Select a category to see items.",
                ["menu.inSet"] = "In the set:",
                ["menu.recommended.eyebrow"] = "Recommended products",
                ["menu.recommended.title"] = "How about something extra?",
                ["menu.recommended.subtitle"] = "Add one of our suggestions before continuing.",
                ["menu.recommended.add"] = "Add to cart",
                ["menu.recommended.goToCart"] = "Go to cart",
                ["menu.recommended.continue"] = "Continue shopping",
                ["menu.setChoice.title"] = "How to add the product?",
                ["menu.setChoice.subtitle"] = "You can add it on its own or pick one of the sets.",
                ["menu.setChoice.noSet"] = "Without set",
                ["menu.setChoice.withSet"] = "Add as set",
                ["menu.setList.title"] = "Choose a set",
                ["menu.setList.subtitle"] = "Only sets containing the selected product are shown.",
                ["menu.setList.close"] = "Close",
                ["menu.modal.add.title"] = "Add product",
                ["menu.modal.quantity"] = "Quantity:",
                ["menu.modal.main"] = "Main ingredients",
                ["menu.modal.addons"] = "Add-ons",
                ["menu.modal.selectedAddons"] = "Selected add-ons:",
                ["menu.modal.cancel"] = "Cancel",
                ["menu.modal.confirm"] = "Add to cart",
                ["cart.title"] = "Cart",
                ["cart.empty"] = "Your cart is empty.",
                ["cart.goMenu"] = "Back to menu",
                ["cart.item"] = "Cart item",
                ["cart.defaultIngredients"] = "Default ingredients",
                ["cart.edit"] = "Edit",
                ["cart.remove"] = "Remove",
                ["cart.summary"] = "Summary",
                ["cart.subtotal"] = "Subtotal",
                ["cart.tax"] = "Tax",
                ["cart.total"] = "Total",
                ["cart.pay"] = "Pay",
                ["cart.back"] = "Back to menu",
                ["cart.modal.editTitle"] = "Edit product",
                ["cart.modal.cancel"] = "Cancel",
                ["cart.modal.save"] = "Save changes",
                ["payment.title"] = "Payment",
                ["payment.eyebrow"] = "Choose payment method",
                ["payment.header"] = "How would you like to pay?",
                ["payment.toPay"] = "To pay:",
                ["payment.card"] = "Card",
                ["payment.card.subtitle"] = "Contactless or chip payment",
                ["payment.cash"] = "Cash",
                ["payment.cash.subtitle"] = "Pay at the register",
                ["payment.blik.subtitle"] = "Enter the code from the app",
                ["payment.summary"] = "Summary",
                ["payment.total"] = "Total",
                ["payment.summary.subtitle"] = "Choose a payment method and then confirm.",
                ["payment.backToCart"] = "Back to cart",
                ["payment.pay"] = "Pay",
                ["payment.success.eyebrow"] = "Thank you for your order",
                ["payment.success.title"] = "Your order number is",
                ["payment.success.note"] = "Show this number when collecting. The order appears on the kitchen screen immediately.",
                ["payment.success.finish"] = "Finish",
                ["payment.success.autoReturn"] = "You will be returned to the start screen shortly.",
                ["kitchen.title"] = "Kitchen",
                ["kitchen.eyebrow"] = "Kitchen panel",
                ["kitchen.waiting"] = "Waiting orders",
                ["kitchen.subtitle"] = "New orders appear automatically – click \"Ready\" once served.",
                ["kitchen.queue"] = "in queue",
                ["kitchen.refresh"] = "Refresh",
                ["kitchen.empty.eyebrow"] = "No orders",
                ["kitchen.empty.title"] = "All orders are completed",
                ["kitchen.empty.subtitle"] = "New orders will show up here as soon as they are placed.",
                ["kitchen.history"] = "Order history",
                ["kitchen.baseIngredients"] = "Base ingredients",
                ["kitchen.addons"] = "Add-ons",
                ["kitchen.none"] = "None",
                ["kitchen.without"] = "without",
                ["kitchen.ready"] = "Ready",
                ["kitchen.collected"] = "Collected",
                ["kitchen.error.ready"] = "Could not mark the order as ready.",
                ["kitchen.error.close"] = "Could not close the order.",
                ["kitchen.error.load"] = "Could not load orders.",
                ["orderDisplay.title"] = "Orders",
                ["orderDisplay.ready"] = "Ready",
                ["orderDisplay.preparing"] = "Preparing",
                ["orderDisplay.noReady"] = "No ready orders right now",
                ["orderDisplay.noPreparing"] = "No orders in preparation",
                ["admin.language.title"] = "Language settings",
                ["admin.language.subtitle"] = "Set the interface language for management screens.",
                ["admin.language.admin"] = "Admin panel",
                ["admin.language.kitchen"] = "Kitchen view",
                ["admin.language.display"] = "Order display",
                ["admin.language.save"] = "Save languages"
            },
            ["de"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["orderType.title"] = "Bestellart wählen",
                ["orderType.subtitle"] = "Sag uns, ob du hier isst oder mitnimmst.",
                ["orderType.eatIn"] = "Vor Ort",
                ["orderType.takeaway"] = "Zum Mitnehmen",
                ["menu.cart"] = "Warenkorb",
                ["menu.addToCart"] = "In den Warenkorb",
                ["menu.chooseCategory"] = "Wähle eine Kategorie, um Produkte zu sehen.",
                ["menu.inSet"] = "Im Set:",
                ["menu.recommended.eyebrow"] = "Empfohlene Produkte",
                ["menu.recommended.title"] = "Wie wäre es mit etwas Extra?",
                ["menu.recommended.subtitle"] = "Füge eine unserer Empfehlungen hinzu, bevor du fortfährst.",
                ["menu.recommended.add"] = "In den Warenkorb",
                ["menu.recommended.goToCart"] = "Zum Warenkorb",
                ["menu.recommended.continue"] = "Weiter einkaufen",
                ["menu.setChoice.title"] = "Wie soll das Produkt hinzugefügt werden?",
                ["menu.setChoice.subtitle"] = "Du kannst es einzeln oder als Set hinzufügen.",
                ["menu.setChoice.noSet"] = "Ohne Set",
                ["menu.setChoice.withSet"] = "Im Set hinzufügen",
                ["menu.setList.title"] = "Set auswählen",
                ["menu.setList.subtitle"] = "Es werden nur Sets mit dem gewählten Produkt angezeigt.",
                ["menu.setList.close"] = "Schließen",
                ["menu.modal.add.title"] = "Produkt hinzufügen",
                ["menu.modal.quantity"] = "Menge:",
                ["menu.modal.main"] = "Hauptzutaten",
                ["menu.modal.addons"] = "Extras",
                ["menu.modal.selectedAddons"] = "Gewählte Extras:",
                ["menu.modal.cancel"] = "Abbrechen",
                ["menu.modal.confirm"] = "In den Warenkorb",
                ["cart.title"] = "Warenkorb",
                ["cart.empty"] = "Dein Warenkorb ist leer.",
                ["cart.goMenu"] = "Zurück zum Menü",
                ["cart.item"] = "Eintrag im Warenkorb",
                ["cart.defaultIngredients"] = "Standardzutaten",
                ["cart.edit"] = "Bearbeiten",
                ["cart.remove"] = "Entfernen",
                ["cart.summary"] = "Zusammenfassung",
                ["cart.subtotal"] = "Zwischensumme",
                ["cart.tax"] = "Steuer",
                ["cart.total"] = "Gesamt",
                ["cart.pay"] = "Bezahlen",
                ["cart.back"] = "Zurück zum Menü",
                ["cart.modal.editTitle"] = "Produkt bearbeiten",
                ["cart.modal.cancel"] = "Abbrechen",
                ["cart.modal.save"] = "Änderungen speichern",
                ["payment.title"] = "Zahlung",
                ["payment.eyebrow"] = "Zahlungsart wählen",
                ["payment.header"] = "Wie möchtest du bezahlen?",
                ["payment.toPay"] = "Zu zahlen:",
                ["payment.card"] = "Karte",
                ["payment.card.subtitle"] = "Kontaktlos oder Chip",
                ["payment.cash"] = "Bar",
                ["payment.cash.subtitle"] = "Zahle an der Kasse",
                ["payment.blik.subtitle"] = "Gib den Code aus der App ein",
                ["payment.summary"] = "Zusammenfassung",
                ["payment.total"] = "Gesamt",
                ["payment.summary.subtitle"] = "Wähle die Zahlungsart und bestätige anschließend.",
                ["payment.backToCart"] = "Zurück zum Warenkorb",
                ["payment.pay"] = "Bezahlen",
                ["payment.success.eyebrow"] = "Danke für die Bestellung",
                ["payment.success.title"] = "Deine Bestellnummer lautet",
                ["payment.success.note"] = "Zeige diese Nummer bei der Abholung. Die Bestellung erscheint sofort auf dem Küchenbildschirm.",
                ["payment.success.finish"] = "Fertig",
                ["payment.success.autoReturn"] = "Wir kehren gleich automatisch zum Startbildschirm zurück.",
                ["kitchen.title"] = "Küche",
                ["kitchen.eyebrow"] = "Küchenpanel",
                ["kitchen.waiting"] = "Ausstehende Bestellungen",
                ["kitchen.subtitle"] = "Neue Bestellungen erscheinen automatisch – klicke \"Fertig\" nach Ausgabe.",
                ["kitchen.queue"] = "in der Warteschlange",
                ["kitchen.refresh"] = "Aktualisieren",
                ["kitchen.empty.eyebrow"] = "Keine Bestellungen",
                ["kitchen.empty.title"] = "Alle Bestellungen sind erledigt",
                ["kitchen.empty.subtitle"] = "Neue Bestellungen erscheinen hier sofort nach Aufgabe.",
                ["kitchen.history"] = "Bestellverlauf",
                ["kitchen.baseIngredients"] = "Grundzutaten",
                ["kitchen.addons"] = "Extras",
                ["kitchen.none"] = "Keine",
                ["kitchen.without"] = "ohne",
                ["kitchen.ready"] = "Fertig",
                ["kitchen.collected"] = "Abgeholt",
                ["kitchen.error.ready"] = "Bestellung konnte nicht als fertig markiert werden.",
                ["kitchen.error.close"] = "Bestellung konnte nicht geschlossen werden.",
                ["kitchen.error.load"] = "Bestellungen konnten nicht geladen werden.",
                ["orderDisplay.title"] = "Bestellungen",
                ["orderDisplay.ready"] = "Bereit",
                ["orderDisplay.preparing"] = "In Vorbereitung",
                ["orderDisplay.noReady"] = "Keine fertigen Bestellungen",
                ["orderDisplay.noPreparing"] = "Keine Bestellungen in Vorbereitung",
                ["admin.language.title"] = "Spracheinstellungen",
                ["admin.language.subtitle"] = "Lege die Sprache für Verwaltungsansichten fest.",
                ["admin.language.admin"] = "Adminbereich",
                ["admin.language.kitchen"] = "Küchenansicht",
                ["admin.language.display"] = "Bestellanzeige",
                ["admin.language.save"] = "Sprachen speichern"
            }
        };

        public static bool TryGetTranslation(string language, string key, out string translation)
        {
            translation = key;
            if (Translations.TryGetValue(language, out var values) && values.TryGetValue(key, out var value))
            {
                translation = value;
                return true;
            }

            return false;
        }
    }
}
