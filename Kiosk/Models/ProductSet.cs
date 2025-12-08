using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kiosk.Models
{
    public class ProductSet
    {
        public int Id { get; set; }

        [Required]
        public int SetMenuItemId { get; set; }

        public MenuItem? SetMenuItem { get; set; }

        public List<ProductSetItem> Items { get; set; } = new();
    }
}