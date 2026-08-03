using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TimekeepingSystem.Models;

/// <summary>
/// Design-time factory dùng cho dotnet-ef CLI.
/// Đọc chuỗi kết nối từ appsettings.json để áp dụng migration vào DB thật.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("default")
            ?? "Host=localhost;Database=timekeeping;Username=postgres;Password=postgres";

        Console.WriteLine($"Using connection string: {connectionString}");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}

