namespace TimekeepingSystem.Models.ViewModels;

public class AttendanceMatrixViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int DaysInMonth { get; set; }
    public bool CanGoPrev { get; set; }
    public bool CanGoNext { get; set; }
    public List<WorkerRow> Workers { get; set; } = new();
}

public class WorkerRow
{
    public User? User { get; set; }
    public Dictionary<int, Attendance?> Days { get; set; } = new(); // day → Attendance
    public int PresentCount { get; set; }
    public int LateCount { get; set; }
    public int AbsentCount { get; set; }
    public int HalfDayCount { get; set; }
    public decimal TotalWorkingHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal TotalSalary { get; set; }
}

