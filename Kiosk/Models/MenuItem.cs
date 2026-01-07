using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kiosk.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        public string Name { get; set; }

        public string? NameEn { get; set; }

        [Required(ErrorMessage = "Opis jest wymagany")]
        public string Description { get; set; }

        public string? DescriptionEn { get; set; }

        [Required(ErrorMessage = "Cena jest wymagana")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi być większa od 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Adres obrazu jest wymagany")]
        public string? Image { get; set; }

        [Required(ErrorMessage = "Kategoria jest wymagana")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }   // 👈 TU ZMIANA

        public List<MenuItemIngredient> Ingredients { get; set; } = new();
    }
}
