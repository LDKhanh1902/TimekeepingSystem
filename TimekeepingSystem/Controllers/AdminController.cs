using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimekeepingSystem.Models;
using TimekeepingSystem.Models.ViewModels;

namespace TimekeepingSystem.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("Role") == "Admin";
    }

    public async Task<IActionResult> Dashboard()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var today = DateTime.Today;
        var workers = await _context.Users.Where(u => u.Role == "Worker" && u.IsActive).ToListAsync();
        var todayAttendances = await _context.Attendances
            .Include(a => a.User)
            .Include(a => a.Shift)
            .Where(a => a.Date == today)
            .OrderByDescending(a => a.CheckIn)
            .ToListAsync();

        var viewModel = new AdminDashboardViewModel
        {
            TotalWorkers = workers.Count,
            TotalPresentsToday = todayAttendances.Count(a => a.Status == "Present"),
            TotalAbsentsToday = workers.Count - todayAttendances.Count,
            TotalLatesToday = todayAttendances.Count(a => a.Status == "Late"),
            TodayAttendances = todayAttendances,
            Workers = workers,
            CurrentMonth = today.Month,
            CurrentYear = today.Year
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Workers()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var workers = await _context.Users
            .Where(u => u.Role == "Worker")
            .OrderByDescending(u => u.IsActive)
            .ThenBy(u => u.FullName)
            .ToListAsync();

        return View(workers);
    }

    public async Task<IActionResult> WorkerAttendance(int id, int? year, int? month)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var today = DateTime.Today;
        int y = year ?? today.Year;
        int m = month ?? today.Month;

        if (m < 1) { m = 1; y--; }
        if (m > 12) { m = 12; y++; }

        var attendances = await _context.Attendances
            .Include(a => a.Shift)
            .Where(a => a.UserId == id && a.Date.Year == y && a.Date.Month == m)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        var daysInMonth = DateTime.DaysInMonth(y, m);
        var allDates = new List<Attendance>();
        for (int d = 1; d <= daysInMonth; d++)
        {
            var date = new DateTime(y, m, d);
            var existing = attendances.FirstOrDefault(a => a.Date == date);
            if (existing != null)
                allDates.Add(existing);
            else
            {
                allDates.Add(new Attendance
                {
                    UserId = id,
                    Date = date,
                    Status = "Absent",
                    ShiftId = 1
                });
            }
        }

        ViewBag.User = user;
        ViewBag.Year = y;
        ViewBag.Month = m;
        ViewBag.CanGoPrev = true;
        ViewBag.CanGoNext = new DateTime(y, m, 1) < new DateTime(today.Year, today.Month, 1);
        ViewBag.Shifts = await _context.Shifts.ToListAsync();

        return View(allDates.OrderBy(a => a.Date).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAttendance(int id, int userId, DateTime date, int shiftId, string status,
        string? checkIn, string? checkOut, decimal overtimeHours, string? notes)
    {
        if (!IsAdmin()) return Unauthorized();

        var attendance = await _context.Attendances.FindAsync(id);
        if (attendance != null)
        {
            attendance.ShiftId = shiftId;
            attendance.Status = status;
            attendance.Notes = notes;
            attendance.OvertimeHours = overtimeHours;
            attendance.CheckIn = ParseTime(date, checkIn, status);
            attendance.CheckOut = ParseTime(date, checkOut, status);
            attendance.UpdatedAt = DateTime.Now;
        }
        else
        {
            attendance = new Attendance
            {
                UserId = userId,
                Date = date,
                ShiftId = shiftId,
                Status = status,
                Notes = notes,
                OvertimeHours = overtimeHours,
                CheckIn = ParseTime(date, checkIn, status),
                CheckOut = ParseTime(date, checkOut, status),
                CreatedAt = DateTime.Now
            };
            _context.Attendances.Add(attendance);
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Cập nhật chấm công thành công!";
        return RedirectToAction("WorkerAttendance", new { id = userId, year = date.Year, month = date.Month });
    }

    [HttpPost]
    public async Task<IActionResult> AddAttendance(int userId, DateTime date, int shiftId, string status,
        string? checkIn, string? checkOut, decimal overtimeHours, string? notes)
    {
        if (!IsAdmin()) return Unauthorized();

        var existing = await _context.Attendances
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == date);

        if (existing != null)
        {
            TempData["Error"] = "Ngày này đã có chấm công!";
            return RedirectToAction("WorkerAttendance", new { id = userId, year = date.Year, month = date.Month });
        }

        var attendance = new Attendance
        {
            UserId = userId,
            Date = date,
            ShiftId = shiftId,
            Status = status,
            Notes = notes,
            OvertimeHours = overtimeHours,
            CheckIn = ParseTime(date, checkIn, status),
            CheckOut = ParseTime(date, checkOut, status),
            CreatedAt = DateTime.Now
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Thêm chấm công thành công!";
        return RedirectToAction("WorkerAttendance", new { id = userId, year = date.Year, month = date.Month });
    }

    private static DateTime? ParseTime(DateTime date, string? time, string status)
    {
        if (status == "Absent" || string.IsNullOrWhiteSpace(time))
            return null;

        if (TimeSpan.TryParse(time, out var ts))
            return date.Date.Add(ts);

        // Default theo ca sáng nếu không nhập
        return status == "Present" || status == "Late" ? date.Date.AddHours(6) : date.Date.AddHours(12);
    }

    public async Task<IActionResult> MonthlyAttendance(int? year, int? month)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var today = DateTime.Today;
        int y = year ?? today.Year;
        int m = month ?? today.Month;

        if (m < 1) { m = 1; y--; }
        if (m > 12) { m = 12; y++; }

        var daysInMonth = DateTime.DaysInMonth(y, m);
        var workers = await _context.Users
            .Where(u => u.Role == "Worker" && u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var attendances = await _context.Attendances
            .Include(a => a.Shift)
            .Where(a => a.Date.Year == y && a.Date.Month == m)
            .ToListAsync();

        var attendanceLookup = attendances
            .GroupBy(a => a.UserId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(a => a.Date.Day));

        var workerRows = new List<WorkerRow>();
        foreach (var worker in workers)
        {
            var row = new WorkerRow
            {
                User = worker,
                Days = new Dictionary<int, Attendance?>()
            };

            var workerAtt = attendanceLookup.GetValueOrDefault(worker.Id, new Dictionary<int, Attendance>());

            for (int d = 1; d <= daysInMonth; d++)
            {
                if (workerAtt.ContainsKey(d))
                    row.Days[d] = workerAtt[d];
                else
                    row.Days[d] = null;
            }

            row.PresentCount = workerAtt.Values.Count(a => a.Status == "Present");
            row.LateCount = workerAtt.Values.Count(a => a.Status == "Late");
            row.AbsentCount = workerAtt.Values.Count(a => a.Status == "Absent");
            row.HalfDayCount = workerAtt.Values.Count(a => a.Status == "HalfDay");
            row.TotalWorkingHours = workerAtt.Values
                .Where(a => a.CheckIn.HasValue && a.CheckOut.HasValue)
                .Sum(a => (decimal)(a.CheckOut!.Value - a.CheckIn!.Value).TotalHours);
            row.TotalOvertimeHours = workerAtt.Values.Sum(a => a.OvertimeHours);

            decimal daysWorked = row.PresentCount + row.LateCount + (row.HalfDayCount * 0.5m);
            decimal dailySalary = worker.BaseSalary / 26m;
            row.TotalSalary = (daysWorked * dailySalary) + (row.TotalOvertimeHours * (dailySalary / 8m) * 1.5m);

            workerRows.Add(row);
        }

        var viewModel = new AttendanceMatrixViewModel
        {
            Year = y,
            Month = m,
            DaysInMonth = daysInMonth,
            CanGoPrev = true,
            CanGoNext = new DateTime(y, m, 1) < new DateTime(today.Year, today.Month, 1),
            Workers = workerRows
        };

        return View(viewModel);
    }

    public async Task<IActionResult> SalarySlip(int id, int? month, int? year)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var today = DateTime.Today;
        int m = month ?? today.Month;
        int y = year ?? today.Year;

        if (m < 1) { m = 12; y--; }
        if (m > 12) { m = 1; y++; }

        var attendances = await _context.Attendances
            .Include(a => a.Shift)
            .Where(a => a.UserId == id && a.Date.Year == y && a.Date.Month == m)
            .OrderBy(a => a.Date)
            .ToListAsync();

        int presentDays = attendances.Count(a => a.Status == "Present");
        int lateDays = attendances.Count(a => a.Status == "Late");
        int absentDays = attendances.Count(a => a.Status == "Absent");
        int halfDays = attendances.Count(a => a.Status == "HalfDay");
        int totalWorkingDays = DateTime.DaysInMonth(y, m);
        decimal baseSalary = user.BaseSalary;
        decimal dailySalary = totalWorkingDays > 0 ? baseSalary / 26m : 0;

        decimal daysWorked = presentDays + lateDays + (halfDays * 0.5m);
        decimal totalOvertimeHours = attendances.Sum(a => a.OvertimeHours);

        // Lương cơ bản theo ngày công, nửa ngày tính 50%
        decimal grossSalary = (daysWorked * dailySalary);
        decimal overtimePay = totalOvertimeHours * (dailySalary / 8m) * 1.5m;

        // Phụ cấp chuyên cần: 600k nếu đi đủ Tổng ngày (không vắng, không muộn)
        const decimal ChuyenCanAmount = 600000m;
        // Phụ cấp trách nhiệm: 400k nếu không vắng ngày nào
        const decimal TrachNhiemAmount = 400000m;

        bool hasFullAttendance = absentDays == 0 && presentDays + lateDays + halfDays >= totalWorkingDays;
        bool noAbsence = absentDays == 0;

        decimal chuyenCanBonus = hasFullAttendance ? ChuyenCanAmount : 0;
        decimal trachNhiemBonus = noAbsence ? TrachNhiemAmount : 0;

        decimal deductions = absentDays * dailySalary; // trừ ngày vắng
        decimal netSalary = grossSalary + chuyenCanBonus + trachNhiemBonus + overtimePay - deductions;

        var viewModel = new SalarySlipViewModel
        {
            User = user,
            Month = m,
            Year = y,
            TotalWorkingDays = totalWorkingDays,
            PresentDays = presentDays,
            LateDays = lateDays,
            AbsentDays = absentDays,
            HalfDays = halfDays,
            DaysWorked = daysWorked,
            SalaryPerShift = user.SalaryPerShift,
            BaseSalary = baseSalary,
            DailySalary = dailySalary,
            GrossSalary = grossSalary,
            Deductions = deductions,
            NetSalary = netSalary < 0 ? 0 : netSalary,
            TotalOvertimeHours = totalOvertimeHours,
            OvertimePay = overtimePay,
            ChuyenCanBonus = chuyenCanBonus,
            TrachNhiemBonus = trachNhiemBonus,
            Attendances = attendances
        };

        return View(viewModel);
    }
}

