using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class OvertimeRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        public TimeSpan EndTime { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot be longer than 500 characters.")]
        public string Reason { get; set; }

        public RequestStatus Status { get; set; }

        public int? ApprovedBy { get; set; }

        public User User { get; set; }
    }
}
