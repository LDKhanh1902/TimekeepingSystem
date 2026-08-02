using System.ComponentModel.DataAnnotations;

namespace TimekeepingSystem.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = "Worker"; // Worker or Admin

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    public decimal SalaryPerShift { get; set; } = 200000; // VND

    public decimal BaseSalary { get; set; } = 5700000; // Lương tháng cơ bản

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}

