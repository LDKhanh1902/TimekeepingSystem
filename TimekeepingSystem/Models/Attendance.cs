using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeepingSystem.Models;

public class Attendance
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }

    public int ShiftId { get; set; }

    [ForeignKey("ShiftId")]
    public Shift? Shift { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public DateTime? CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    [Range(0, 24)]
    public decimal OvertimeHours { get; set; } = 0; // Số giờ tăng ca

    [MaxLength(50)]
    public string Status { get; set; } = "Present"; // Present, Late, Absent, HalfDay

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }
}

