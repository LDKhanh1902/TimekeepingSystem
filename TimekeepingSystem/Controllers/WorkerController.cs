using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimekeepingSystem.Models;
using TimekeepingSystem.Models.ViewModels;

namespace TimekeepingSystem.Controllers;

public class WorkerController : Controller
{
    private readonly AppDbContext _context;

    public WorkerController(AppDbContext context)
    {
        _context = context;
    }

    private bool IsLoggedIn()
    {
        return HttpContext.Session.GetString("UserId") != null;
    }

    private string GetRole()
    {
        return HttpContext.Session.GetString("Role") ?? "";
    }

    private int GetUserId()
    {
        return int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
    }

    public async Task<IActionResult> Dashboard()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        if (GetRole() != "Worker") return RedirectToAction("Dashboard", "Admin");

        var userId = GetUserId();
        var today = DateTime.Today;

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return RedirectToAction("Logout", "Auth");

        var todayAttendance = await _context.Attendances
            .Include(a => a.Shift)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == today);

        var thisMonthAttendances = await _context.Attendances
            .Include(a => a.Shift)
            .Where(a => a.UserId == userId && a.Date.Year == today.Year && a.Date.Month == today.Month)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        var viewModel = new AttendanceViewModel
        {
            User = user,
            Attendances = thisMonthAttendances,
            Year = today.Year,
            Month = today.Month,
            DaysInMonth = DateTime.DaysInMonth(today.Year, today.Month),
            PresentCount = thisMonthAttendances.Count(a => a.Status == "Present"),
            LateCount = thisMonthAttendances.Count(a => a.Status == "Late"),
            AbsentCount = thisMonthAttendances.Count(a => a.Status == "Absent"),
            HalfDayCount = thisMonthAttendances.Count(a => a.Status == "HalfDay"),
            TotalWorkingHours = thisMonthAttendances
                .Where(a => a.CheckIn.HasValue && a.CheckOut.HasValue)
                .Sum(a => (decimal)(a.CheckOut!.Value - a.CheckIn!.Value).TotalHours),
            TotalOvertimeHours = thisMonthAttendances.Sum(a => a.OvertimeHours),
            TotalSalary = thisMonthAttendances
                .Where(a => a.Status == "Present" || a.Status == "Late")
                .Sum(a => user.SalaryPerShift),
            CanGoNext = false,
            CanGoPrev = true
        };

        ViewBag.TodayAttendance = todayAttendance;
        ViewBag.User = user;

        return View(viewModel);
    }

    public async Task<IActionResult> Profile()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return RedirectToAction("Logout", "Auth");

        return View(user);
    }

    public async Task<IActionResult> Attendance(int? year, int? month)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return RedirectToAction("Logout", "Auth");

        var today = DateTime.Today;
        int y = year ?? today.Year;
        int m = month ?? today.Month;

        // Validate month
        if (m < 1) { m = 1; y--; }
        if (m > 12) { m = 12; y++; }
        if (y < 2024) y = 2024;
        if (y > today.Year + 1) y = today.Year + 1;

        var attendances = await _context.Attendances
            .Include(a => a.Shift)
            .Where(a => a.UserId == userId && a.Date.Year == y && a.Date.Month == m)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        var viewModel = new AttendanceViewModel
        {
            User = user,
            Attendances = attendances,
            Year = y,
            Month = m,
            DaysInMonth = DateTime.DaysInMonth(y, m),
            PresentCount = attendances.Count(a => a.Status == "Present"),
            LateCount = attendances.Count(a => a.Status == "Late"),
            AbsentCount = attendances.Count(a => a.Status == "Absent"),
            HalfDayCount = attendances.Count(a => a.Status == "HalfDay"),
            TotalWorkingHours = attendances
                .Where(a => a.CheckIn.HasValue && a.CheckOut.HasValue)
                .Sum(a => (decimal)(a.CheckOut!.Value - a.CheckIn!.Value).TotalHours),
            TotalOvertimeHours = attendances.Sum(a => a.OvertimeHours),
            TotalSalary = attendances
                .Where(a => a.Status == "Present" || a.Status == "Late")
                .Sum(a => user.SalaryPerShift),
            CanGoNext = new DateTime(y, m, 1) < new DateTime(today.Year, today.Month, 1),
            CanGoPrev = true
        };

        return View(viewModel);
    }
}

