using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using TimekeepingSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("default")
    ?? "Host=localhost;Database=timekeeping;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Auto-create and seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Supabase luôn có sẵn các bảng nội bộ (auth, storage...), nên HasTables()
    // sẽ trả về true kể cả khi bảng của ứng dụng chưa tồn tại.
    // Vì vậy ta kiểm tra trực tiếp bảng "Users" trong schema hiện tại.
    var usersTableExists = context.Database
        .SqlQueryRaw<bool>(
            "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = current_schema() AND table_name = 'Users') AS \"Value\"")
        .AsEnumerable()
        .First();

    if (!usersTableExists)
    {
        var creator = context.GetService<IRelationalDatabaseCreator>();
        creator.CreateTables();
    }

    // Seed: đảm bảo 5 công nhân + tạo bảng chấm công tháng trước & hiện tại
    SeedData.Initialize(context);
}

app.Run();

