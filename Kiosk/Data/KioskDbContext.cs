using Microsoft.EntityFrameworkCore;
using Kiosk.Models;

namespace Kiosk.Data
{
    public class KioskDbContext : DbContext
    {
        public KioskDbContext(DbContextOptions<KioskDbContext> options) : base(options) { }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<MenuItemIngredient> MenuItemIngredients { get; set; }
        public DbSet<LockScreenBackground> LockScreenBackgrounds { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }
        public DbSet<RecommendedProduct> RecommendedProducts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany()
                .HasForeignKey(oi => oi.MenuItemId);
            modelBuilder.Entity<MenuItemIngredient>()
                .HasOne(mi => mi.MenuItem)
                .WithMany(m => m.Ingredients)
                .HasForeignKey(mi => mi.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RecommendedProduct>()
                .HasOne(rp => rp.MenuItem)
                .WithMany()
                .HasForeignKey(rp => rp.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecommendedProduct>()
                .HasIndex(rp => rp.MenuItemId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }

    }
}