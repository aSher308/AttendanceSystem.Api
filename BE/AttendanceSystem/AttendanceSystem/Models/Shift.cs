using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class Shift
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Shift name is required.")]
        [StringLength(100, ErrorMessage = "Shift name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
        public string Description { get; set; }
    }
}
