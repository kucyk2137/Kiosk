using Kiosk.Data;
using Kiosk.Models;
using Kiosk.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// EF Core
builder.Services.AddDbContext<KioskDbContext>(options =>
options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddRazorPages();
builder.Services.AddSession(); // potrzebne dla koszyka
builder.Services.AddSingleton<OrderUpdateNotifier>(); //odœwie¿anie widoku orderdisplay
builder.Services.AddScoped<SiteSettingsService>();
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

app.MapPost("/api/orders/{id:int}/ready", async (int id, KioskDbContext db, OrderUpdateNotifier notifier) =>
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

    order.IsReady = true;
    await db.SaveChangesAsync();
    notifier.Notify(new OrderUpdate(order.Id, order.IsReady, order.IsClosed));
    return Results.NoContent();
});


app.MapPost("/api/orders/{id:int}/complete", async (int id, KioskDbContext db, OrderUpdateNotifier notifier) =>
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

    order.IsReady = true;
    order.IsClosed = true;
    await db.SaveChangesAsync();
    notifier.Notify(new OrderUpdate(order.Id, order.IsReady, order.IsClosed));
    return Results.NoContent();
});

app.MapGet("/api/orders/updates", async (HttpContext context, OrderUpdateNotifier notifier) =>
{
    context.Response.Headers.CacheControl = "no-cache";
    context.Response.Headers.ContentType = "text/event-stream";

    var (id, reader) = notifier.Subscribe();

    await context.Response.WriteAsync("retry: 5000\n\n");
    await context.Response.Body.FlushAsync();

    try
    {
        await foreach (var update in reader.ReadAllAsync(context.RequestAborted))
        {
            var payload = JsonSerializer.Serialize(update);
            await context.Response.WriteAsync($"data: {payload}\n\n");
            await context.Response.Body.FlushAsync();
        }
    }
    catch (OperationCanceledException)
    {
        // Client disconnected
    }
    finally
    {
        notifier.Unsubscribe(id);
    }
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
        OrderNumber = o.OrderNumber,
        OrderType = o.OrderType,
        IsReady = o.IsReady,
        IsClosed = o.IsClosed,
        Items = o.Items
            .Where(i => i.MenuItem != null)
            .Select(i => new KitchenOrderItemDto
            {
                DishName = i.MenuItem.Name,
                Quantity = i.Quantity,
                UnitPrice = i.MenuItem.Price,
                Ingredients = i.SelectedIngredients,
                DefaultIngredients = i.MenuItem.Ingredients
                    .Where(ing => ing.IsDefault)
                    .Select(ing => ing.Name)
                    .ToList(),
                OptionalIngredients = i.MenuItem.Ingredients
                    .Where(ing => !ing.IsDefault)
                    .Select(ing => ing.Name)
                    .ToList()
            })
            .ToList()
    });
