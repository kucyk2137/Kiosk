using System.ComponentModel.DataAnnotations;

namespace Kiosk.Models
{
    public class MenuItemIngredient
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsDefault { get; set; }

        public int MenuItemId { get; set; }
        public decimal AdditionalPrice { get; set; }
        public MenuItem MenuItem { get; set; }
    }
}