namespace TimekeepingSystem.Models.ViewModels;

public class SalarySlipViewModel
{
    public User? User { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int LateDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public decimal DaysWorked { get; set; }
    public decimal SalaryPerShift { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal DailySalary { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal ChuyenCanBonus { get; set; }
    public decimal TrachNhiemBonus { get; set; }
    public List<Attendance> Attendances { get; set; } = new();
}

