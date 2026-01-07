using Kiosk.Data;
using Kiosk.Models;
using Kiosk.Extensions;
using Kiosk.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// EF Core
builder.Services.AddDbContext<KioskDbContext>(options =>
options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddSession(); // potrzebne dla koszyka
builder.Services.AddSingleton<OrderUpdateNotifier>(); //odświeżanie widoku orderdisplay
builder.Services.AddScoped<SiteSettingsService>();
var app = builder.Build();


app.UseStaticFiles();
var supportedCultures = new[]
{
    new CultureInfo("pl-PL"),
    new CultureInfo("en-US")
};
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pl-PL"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};
localizationOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
app.UseRequestLocalization(localizationOptions);
app.UseRouting();
app.UseSession();
app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    if (path.StartsWithSegments("/Admin", out var remaining) &&
        !remaining.StartsWithSegments("/Login") &&
        context.Session.GetString("IsAdmin") != "true")
    {
        context.Response.Redirect("/Admin/Login");
        return;
    }

    await next();
});
app.MapGet("/api/orders", async (KioskDbContext db) =>
{
    var orders = await MapOrders(db, db.Orders.Where(o => !o.IsClosed));

    return Results.Ok(orders);
});

app.MapGet("/api/orders/history", async (KioskDbContext db) =>
{
    var orders = await MapOrders(db, db.Orders.Where(o => o.IsClosed));
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
        return Results.BadRequest(new { message = "Zamówienie zostało już zamknięte." });
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
        return Results.BadRequest(new { message = "Zamówienie zostało już zamknięte." });
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
static async Task<List<KitchenOrderDto>> MapOrders(KioskDbContext db, IQueryable<Order> queryable)
{
    var orders = await queryable
        .Include(o => o.Items)
            .ThenInclude(i => i.MenuItem)
                .ThenInclude(mi => mi.Ingredients)
        .OrderByDescending(o => o.OrderDate)
        .ToListAsync();

    var menuItemIds = orders
        .SelectMany(o => o.Items)
        .Select(i => i.MenuItem?.Id)
        .Where(id => id.HasValue)
        .Select(id => id!.Value)
        .ToHashSet();

    var setLookup = await db.ProductSets
        .Where(ps => menuItemIds.Contains(ps.SetMenuItemId))
        .Include(ps => ps.Items)
            .ThenInclude(psi => psi.MenuItem)
                .ThenInclude(mi => mi.Ingredients)
        .ToDictionaryAsync(ps => ps.SetMenuItemId);

    return orders.Select(o => new KitchenOrderDto
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
            .Select(i => MapKitchenOrderItem(i, setLookup))
            .ToList()
    }).ToList();
}

static KitchenOrderItemDto MapKitchenOrderItem(OrderItem item, Dictionary<int, ProductSet> setLookup)
{
    var menuItem = item.MenuItem!;
    var culture = CultureInfo.CurrentUICulture;
    var (defaultIngredients, optionalIngredients) = BuildIngredientLists(menuItem, setLookup, culture);

    return new KitchenOrderItemDto
    {
        DishName = menuItem.GetDisplayName(culture),
        Quantity = item.Quantity,
        UnitPrice = menuItem.Price,
        Ingredients = item.SelectedIngredients,
        DefaultIngredients = defaultIngredients,
        OptionalIngredients = optionalIngredients
    };
}

static (List<string> defaults, List<string> optionals) BuildIngredientLists(MenuItem menuItem, Dictionary<int, ProductSet> setLookup, CultureInfo culture)
{
    if (setLookup.TryGetValue(menuItem.Id, out var productSet))
    {
        var setDefaults = new List<string>();
        var setOptionals = new List<string>();

        foreach (var setItem in productSet.Items.Where(si => si.MenuItem != null))
        {
            var productName = setItem.MenuItem!.GetDisplayName(culture);

            setDefaults.AddRange(setItem.MenuItem.Ingredients
                .Where(ing => ing.IsDefault)
                .Select(ing => $"{productName}: {ing.GetDisplayName(culture)}"));

            setOptionals.AddRange(setItem.MenuItem.Ingredients
                .Where(ing => !ing.IsDefault)
                .Select(ing => $"{productName}: {ing.GetDisplayName(culture)}"));
        }

        return (setDefaults, setOptionals);
    }

    var defaults = menuItem.Ingredients
        .Where(ing => ing.IsDefault)
        .Select(ing => ing.GetDisplayName(culture))
        .ToList();

    var optionals = menuItem.Ingredients
        .Where(ing => !ing.IsDefault)
        .Select(ing => ing.GetDisplayName(culture))
        .ToList();

    return (defaults, optionals);
}