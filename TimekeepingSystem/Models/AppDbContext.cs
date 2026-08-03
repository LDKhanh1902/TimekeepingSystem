using Microsoft.EntityFrameworkCore;

namespace TimekeepingSystem.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Attendance> Attendances => Set<Attendance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            // Index cho truy vấn lọc theo vai trò (Admin/Worker) + trạng thái active
            entity.HasIndex(u => new { u.Role, u.IsActive });
            entity.Property(u => u.CreatedAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasIndex(a => new { a.UserId, a.Date }).IsUnique();
            // Index riêng cho cột Date: tăng tốc truy vấn theo ngày/tháng (Dashboard, MonthlyAttendance)
            entity.HasIndex(a => a.Date);
            entity.Property(a => a.Date).HasColumnType("timestamp without time zone");
            entity.Property(a => a.CheckIn).HasColumnType("timestamp without time zone");
            entity.Property(a => a.CheckOut).HasColumnType("timestamp without time zone");
            entity.Property(a => a.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(a => a.UpdatedAt).HasColumnType("timestamp without time zone");
            entity.HasOne(a => a.User)
                  .WithMany(u => u.Attendances)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.Shift)
                  .WithMany(s => s.Attendances)
                  .HasForeignKey(a => a.ShiftId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed shifts
        modelBuilder.Entity<Shift>().HasData(
            new Shift { Id = 1, Name = "Ca sáng", StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(12, 0, 0), Description = "06:00 - 12:00" },
            new Shift { Id = 2, Name = "Ca chiều", StartTime = new TimeSpan(12, 0, 0), EndTime = new TimeSpan(18, 0, 0), Description = "12:00 - 18:00" },
            new Shift { Id = 3, Name = "Ca tối", StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(22, 0, 0), Description = "18:00 - 22:00" }
        );

        // Seed users
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "Admin",
                Username = "admin",
                Password = "admin123",
                Role = "Admin",
                Phone = "0123456789",
                Email = "admin@company.com",
                Address = "VP Công ty",
                SalaryPerShift = 0,
                BaseSalary = 0,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1)
            },
            new User
            {
                Id = 2,
                FullName = "Nguyễn Văn A",
                Username = "worker1",
                Password = "123456",
                Role = "Worker",
                Phone = "0909123456",
                Email = "nguyenvana@company.com",
                Address = "Hà Nội",
                SalaryPerShift = 250000,
                BaseSalary = 5700000,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1)
            },
            new User
            {
                Id = 3,
                FullName = "Trần Thị B",
                Username = "worker2",
                Password = "123456",
                Role = "Worker",
                Phone = "0909987654",
                Email = "tranthib@company.com",
                Address = "Hồ Chí Minh",
                SalaryPerShift = 250000,
                BaseSalary = 5700000,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1)
            },
            new User
            {
                Id = 4,
                FullName = "Lê Văn C",
                Username = "worker3",
                Password = "123456",
                Role = "Worker",
                Phone = "0912345678",
                Email = "levanc@company.com",
                Address = "Đà Nẵng",
                SalaryPerShift = 200000,
                BaseSalary = 5700000,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1)
            },
            new User
            {
                Id = 5,
                FullName = "Phạm Thị D",
                Username = "worker4",
                Password = "123456",
                Role = "Worker",
                Phone = "0934567890",
                Email = "phamthid@company.com",
                Address = "Hải Phòng",
                SalaryPerShift = 250000,
                BaseSalary = 6000000,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1)
            },
            new User
            {
                Id = 6,
                FullName = "Hoàng Văn E",
                Username = "worker5",
                Password = "123456",
                Role = "Worker",
                Phone = "0967890123",
                Email = "hoangvane@company.com",
                Address = "Cần Thơ",
                SalaryPerShift = 220000,
                BaseSalary = 5800000,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1)
            }
        );
    }
}

