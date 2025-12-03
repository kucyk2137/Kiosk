using Kiosk.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddDbContext<KioskDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("KioskDb")));

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Ensure database created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KioskDbContext>();
    db.Database.EnsureCreated();
}
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.MapRazorPages();
app.MapFallbackToPage("/LockScreen");
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapRazorPages();
app.Run();
