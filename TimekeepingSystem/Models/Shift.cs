using System.ComponentModel.DataAnnotations;

namespace TimekeepingSystem.Models;

public class Shift
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    // Navigation
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}

