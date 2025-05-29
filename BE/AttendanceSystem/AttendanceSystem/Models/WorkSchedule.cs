using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class WorkSchedule
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Shift ID is required.")]
        public int ShiftId { get; set; }

        [Required(ErrorMessage = "Work date is required.")]
        public DateTime WorkDate { get; set; }

        [StringLength(500, ErrorMessage = "Note cannot be longer than 500 characters.")]
        public string Note { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status cannot be longer than 50 characters.")]
        public string Status { get; set; }

        public User User { get; set; }

        public Shift Shift { get; set; }
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}
