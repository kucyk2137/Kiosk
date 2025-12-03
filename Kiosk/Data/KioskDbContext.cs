using Microsoft.EntityFrameworkCore;
using Kiosk.Models;

namespace Kiosk.Data
{
    public class KioskDbContext : DbContext
    {
        public KioskDbContext(DbContextOptions<KioskDbContext> options) : base(options) { }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Category> Categories { get; set; }

    }
}
