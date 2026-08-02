using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using TimekeepingSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("default")
    ?? "Host=localhost;Database=timekeeping;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Data Protection
builder.Services.AddDataProtection()
    .SetApplicationName("TimekeepingSystem");

// Memory Cache
builder.Services.AddDistributedMemoryCache();

// Session
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Timekeeping.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

// Tự tạo database nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.SetCommandTimeout(60);

    if (db.Database.GetPendingMigrations().Any())
    {
        db.Database.Migrate();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();