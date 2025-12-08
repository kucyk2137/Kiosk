namespace Kiosk.Models
{
    public class ProductSetItem
    {
        public int Id { get; set; }
        public int ProductSetId { get; set; }
        public ProductSet ProductSet { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }
    }
}