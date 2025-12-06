using Kiosk.Data;
using Kiosk.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// EF Core
builder.Services.AddDbContext<KioskDbContext>(options =>
options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddRazorPages();
builder.Services.AddSession(); // potrzebne dla koszyka
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapGet("/api/orders", async (KioskDbContext db) =>
{
    var orders = await db.Orders
        .Include(o => o.Items)
        .ThenInclude(i => i.MenuItem)
        .OrderByDescending(o => o.OrderDate)
        .Select(o => new KitchenOrderDto
        {
            OrderId = o.Id,
            OrderDate = o.OrderDate,
            PaymentMethod = o.PaymentMethod,
            OrderType = o.OrderType,
            Items = o.Items
                .Where(i => i.MenuItem != null)
                .Select(i => new KitchenOrderItemDto
                {
                    DishName = i.MenuItem.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.MenuItem.Price,
                    Ingredients = i.SelectedIngredients
                })
                .ToList()
        })
        .ToListAsync();

    return Results.Ok(orders);
});

app.MapRazorPages();
app.Run();
