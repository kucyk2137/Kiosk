using System.ComponentModel.DataAnnotations;

namespace Kiosk.Models
{
    public class MenuItemIngredient
    {
        public int Id { get; set; }

      
        public string Name { get; set; }

        public bool IsDefault { get; set; }

        public int MenuItemId { get; set; }
        public decimal AdditionalPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public MenuItem MenuItem { get; set; }
    }
}