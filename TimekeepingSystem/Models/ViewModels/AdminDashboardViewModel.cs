namespace TimekeepingSystem.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalWorkers { get; set; }
    public int TotalPresentsToday { get; set; }
    public int TotalAbsentsToday { get; set; }
    public int TotalLatesToday { get; set; }
    public List<Attendance> TodayAttendances { get; set; } = new();
    public List<User> Workers { get; set; } = new();
    public int CurrentMonth { get; set; }
    public int CurrentYear { get; set; }
}

