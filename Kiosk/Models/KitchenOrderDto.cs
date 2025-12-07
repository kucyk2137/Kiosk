using System;
using System.Collections.Generic;

namespace Kiosk.Models
{
    public class KitchenOrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderType { get; set; }

        public bool IsClosed { get; set; }
        public List<KitchenOrderItemDto> Items { get; set; } = new();
    }

    public class KitchenOrderItemDto
    {
        public string DishName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public string Ingredients { get; set; }
        public List<string> DefaultIngredients { get; set; } = new();
        public List<string> OptionalIngredients { get; set; } = new();
    }
}
