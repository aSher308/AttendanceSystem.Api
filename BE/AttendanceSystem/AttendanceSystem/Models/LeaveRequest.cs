using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "From Date is required.")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To Date is required.")]
        public DateTime ToDate { get; set; }

        [Required(ErrorMessage = "Leave type is required.")]
        public LeaveType LeaveType { get; set; } // => enum mới

        [StringLength(500, ErrorMessage = "Reason cannot be longer than 500 characters.")]
        public string Reason { get; set; }

        public RequestStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public int? ApprovedBy { get; set; }

        [StringLength(300, ErrorMessage = "Reviewer comment cannot be longer than 300 characters.")]
        public string ReviewerComment { get; set; }

        // Trường hợp người khác tạo hộ đơn nghỉ
        public int? CreatedBy { get; set; }

        public User User { get; set; }
    }
}
