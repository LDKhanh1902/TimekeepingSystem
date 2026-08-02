namespace TimekeepingSystem.Models.ViewModels;

public class AttendanceViewModel
{
    public User? User { get; set; }
    public List<Attendance> Attendances { get; set; } = new();
    public int Year { get; set; }
    public int Month { get; set; }
    public int DaysInMonth { get; set; }
    public int PresentCount { get; set; }
    public int LateCount { get; set; }
    public int AbsentCount { get; set; }
    public int HalfDayCount { get; set; }
    public decimal TotalWorkingHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal TotalSalary { get; set; }
    public bool CanGoNext { get; set; }
    public bool CanGoPrev { get; set; }
}

