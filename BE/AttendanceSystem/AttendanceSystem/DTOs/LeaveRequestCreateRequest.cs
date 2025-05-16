using AttendanceSystem.Models;

namespace AttendanceSystem.DTOs
{
    public class LeaveRequestCreateRequest
    {
        public int UserId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public LeaveType LeaveType { get; set; }
        public string Reason { get; set; }
        public int? CreatedBy { get; set; } // nếu người khác tạo hộ
    }
}
