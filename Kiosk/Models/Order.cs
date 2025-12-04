using System;
using System.Collections.Generic;

namespace Kiosk.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public List<OrderItem> Items { get; set; } = new();
    }
}
