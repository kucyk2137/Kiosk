using System.ComponentModel.DataAnnotations;

namespace Kiosk.Models
{
    public class RecommendedProduct
    {
        public int Id { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        public MenuItem? MenuItem { get; set; }
    }
}