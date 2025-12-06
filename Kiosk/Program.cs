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
    var orders = await MapOrders(db.Orders.Where(o => !o.IsClosed)).ToListAsync();

    return Results.Ok(orders);
});

app.MapGet("/api/orders/history", async (KioskDbContext db) =>
{
    var orders = await MapOrders(db.Orders.Where(o => o.IsClosed)).ToListAsync();
    return Results.Ok(orders);
});

app.MapPost("/api/orders/{id:int}/complete", async (int id, KioskDbContext db) =>
{
    var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);

    if (order is null)
    {
        return Results.NotFound();
    }

    if (order.IsClosed)
    {
        return Results.BadRequest(new { message = "Zamówienie zosta³o ju¿ zamkniête." });
    }

    order.IsClosed = true;
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapRazorPages();
app.Run();
static IQueryable<KitchenOrderDto> MapOrders(IQueryable<Order> queryable) => queryable
    .Include(o => o.Items)
    .ThenInclude(i => i.MenuItem)
    .OrderByDescending(o => o.OrderDate)
    .Select(o => new KitchenOrderDto
    {
        OrderId = o.Id,
        OrderDate = o.OrderDate,
        PaymentMethod = o.PaymentMethod,
        OrderType = o.OrderType,
        IsClosed = o.IsClosed,
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
    });
