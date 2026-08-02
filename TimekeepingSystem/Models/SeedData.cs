using Microsoft.EntityFrameworkCore;

namespace TimekeepingSystem.Models;

/// <summary>
/// Bộ sinh dữ liệu: đảm bảo 5 công nhân tồn tại và tạo bảng chấm công
/// cho tháng trước (cả tháng) và tháng hiện tại (đến ngày hôm nay).
/// Idempotent: chạy lại nhiều lần không tạo dữ liệu trùng.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Danh sách 5 công nhân mặc định.
    /// </summary>
    private static readonly (string Username, string FullName, string Phone, string Email, string Address, decimal SalaryPerShift, decimal BaseSalary)[] Workers =
    {
        ("worker1", "Nguyễn Văn A", "0909123456", "nguyenvana@company.com", "Hà Nội", 250000m, 5700000m),
        ("worker2", "Trần Thị B", "0909987654", "tranthib@company.com", "Hồ Chí Minh", 250000m, 5700000m),
        ("worker3", "Lê Văn C", "0912345678", "levanc@company.com", "Đà Nẵng", 200000m, 5700000m),
        ("worker4", "Phạm Thị D", "0934567890", "phamthid@company.com", "Hải Phòng", 250000m, 6000000m),
        ("worker5", "Hoàng Văn E", "0967890123", "hoangvane@company.com", "Cần Thơ", 220000m, 5800000m),
    };

    public static void Initialize(AppDbContext context)
    {
        // 1. Đảm bảo các ca làm việc tồn tại (1: Sáng, 2: Chiều, 3: Tối)
        var shifts = context.Shifts.OrderBy(s => s.Id).ToList();
        if (shifts.Count < 3)
        {
            // Không thể tự thêm ca nếu chưa có - thường đã có sẵn qua HasData
            return;
        }

        // 2. Đảm bảo 5 công nhân tồn tại
        var existingUsers = context.Users.ToList();
        int nextId = existingUsers.Count > 0 ? existingUsers.Max(u => u.Id) : 0;

        foreach (var w in Workers)
        {
            var exists = existingUsers.FirstOrDefault(u => u.Username == w.Username);
            if (exists != null)
            {
                // Cập nhật lương/phòng nếu cần
                exists.SalaryPerShift = w.SalaryPerShift;
                exists.BaseSalary = w.BaseSalary;
                continue;
            }

            nextId++;
            context.Users.Add(new User
            {
                Id = nextId,
                Username = w.Username,
                FullName = w.FullName,
                Password = "123456",
                Role = "Worker",
                Phone = w.Phone,
                Email = w.Email,
                Address = w.Address,
                SalaryPerShift = w.SalaryPerShift,
                BaseSalary = w.BaseSalary,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1)
            });
        }
        context.SaveChanges();

        // 3. Sinh chấm công cho tháng trước và tháng hiện tại
        var today = DateTime.Today;
        var prevMonth = today.AddMonths(-1);
        var startDate = new DateTime(prevMonth.Year, prevMonth.Month, 1);
        var endDate = today;

        var workers = context.Users.Where(u => u.Role == "Worker" && u.IsActive).OrderBy(u => u.Id).ToList();
        var existingAtt = context.Attendances
            .Where(a => a.Date >= startDate && a.Date <= endDate)
            .Select(a => new { a.UserId, a.Date })
            .ToList();
        var existingKeys = new HashSet<(int, DateTime)>(existingAtt.Select(x => (x.UserId, x.Date.Date)));

        // Bộ sinh số ngẫu nhiên ổn định (deterministic) theo từng user & ngày
        var addedCount = 0;
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            // Nghỉ Chủ nhật
            if (date.DayOfWeek == DayOfWeek.Sunday) continue;

            // Không sinh chấm công cho ngày trong tương lai
            if (date > today) continue;

            foreach (var user in workers)
            {
                var key = (user.Id, date.Date);
                if (existingKeys.Contains(key)) continue;

                var rnd = new Random(user.Id * 100000 + date.Year * 1000 + date.Month * 100 + date.Day * 7);
                var roll = rnd.Next(100);

                string status;
                DateTime? checkIn = null;
                DateTime? checkOut = null;
                decimal overtime = 0m;
                string? notes = null;

                if (roll < 70) // Present
                {
                    status = "Present";
                }
                else if (roll < 82) // Late
                {
                    status = "Late";
                }
                else if (roll < 90) // HalfDay
                {
                    status = "HalfDay";
                }
                else // Absent
                {
                    status = "Absent";
                    notes = "Nghỉ không phép";
                }

                // Chọn ca ngẫu nhiên
                var shift = shifts[rnd.Next(shifts.Count)];
                int shiftStart = shift.StartTime.Hours;

                if (status == "Present")
                {
                    checkIn = date.Date.AddHours(shiftStart).AddMinutes(rnd.Next(0, 15));
                    checkOut = date.Date.Add(shift.EndTime).AddMinutes(rnd.Next(-5, 5));
                    // Tăng ca 30% số ngày
                    if (rnd.Next(100) < 30)
                    {
                        overtime = rnd.Next(1, 4);
                        checkOut = checkOut.Value.AddHours((double)overtime);
                        notes = "Tăng ca " + (int)overtime + " giờ";
                    }
                }
                else if (status == "Late")
                {
                    checkIn = date.Date.AddHours(shiftStart).AddMinutes(rnd.Next(20, 90));
                    checkOut = date.Date.Add(shift.EndTime).AddMinutes(rnd.Next(-5, 5));
                    notes = "Đi muộn";
                }
                else if (status == "HalfDay")
                {
                    // Làm nửa buổi (chỉ có check-in)
                    checkIn = date.Date.AddHours(shiftStart).AddMinutes(rnd.Next(0, 15));
                    checkOut = null;
                    notes = "Nghỉ nửa ngày";
                }

                context.Attendances.Add(new Attendance
                {
                    UserId = user.Id,
                    ShiftId = shift.Id,
                    Date = date.Date,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    OvertimeHours = overtime,
                    Status = status,
                    Notes = notes,
                    CreatedAt = DateTime.Now
                });
                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            context.SaveChanges();
        }
    }
}

