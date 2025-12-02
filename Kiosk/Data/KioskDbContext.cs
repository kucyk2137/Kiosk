using Kiosk.Models;
using Microsoft.EntityFrameworkCore;

namespace Kiosk.Data
{
    public class KioskDbContext : DbContext
    {
        public KioskDbContext(DbContextOptions<KioskDbContext> options) : base(options) { }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
