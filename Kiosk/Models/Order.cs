using System;
using System.Collections.Generic;

namespace Kiosk.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string PaymentMethod { get; set; } // np. "Karta", "Gotówka"
        public List<OrderItem> Items { get; set; } = new();
    }
}
