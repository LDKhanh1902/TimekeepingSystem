using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using TimekeepingSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("default")
    ?? "Host=localhost;Database=timekeeping;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);

        npgsql.CommandTimeout(30);
    }));

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
    // SameAsRequest: vẫn gửi cookie qua HTTPS (Render), nhưng không chặn khi dev chạy HTTP local
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

// Xử lý đúng scheme (HTTPS) khi chạy sau reverse proxy (Render, Azure, Nginx...)
// Giúp tránh redirect loop HTTP <-> HTTPS
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { },
    KnownProxies = { }
});

// Tự tạo database nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.SetCommandTimeout(30);

        // Kiểm tra các migration đã được áp dụng
        var appliedMigrations = new List<string>();
        try
        {
            appliedMigrations = db.Database.GetAppliedMigrations().ToList();
        }
        catch
        {
            // Bảng __EFMigrationsHistory chưa tồn tại => DB chưa từng được migrate
        }

        // Phát hiện DB cũ: có bảng Shifts nhưng KHÔNG có migration history
        // (trường hợp trước đây dùng EnsureCreated() hoặc seed thủ công)
        bool hasLegacyTables = false;
        if (appliedMigrations.Count == 0)
        {
            try
            {
                hasLegacyTables = db.Shifts.Any();
            }
            catch
            {
                // Bảng chưa tồn tại => DB mới, không cần drop
            }
        }

        if (hasLegacyTables)
        {
            logger.LogWarning("Phát hiện DB cũ (bảng đã tồn tại nhưng chưa có migration history). " +
                              "Đang drop bảng cũ để migrate lại sạch...");
            db.Database.ExecuteSqlRaw(
                "DROP TABLE IF EXISTS \"Attendances\" CASCADE; " +
                "DROP TABLE IF EXISTS \"Users\" CASCADE; " +
                "DROP TABLE IF EXISTS \"Shifts\" CASCADE;");
        }

        logger.LogInformation("Đang kiểm tra/áp dụng migrations...");
        db.Database.Migrate();
        logger.LogInformation("Migrations đã được áp dụng thành công.");
    }
    catch (Exception ex)
    {
        // Không crash app: log rõ lỗi để debug, app vẫn khởi động để Render health check pass
        // (tránh vòng lặp restart -> timeout trên Render)
        logger.LogError(ex, "LỖI KHỞI TẠO DATABASE (app vẫn khởi động, kiểm tra connection string): {Message}", ex.Message);
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