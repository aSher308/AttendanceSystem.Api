using AttendanceSystem.Models;

namespace AttendanceSystem.DTOs
{
    public class LeaveRequestApprovalRequest
    {
        public int Id { get; set; }
        public RequestStatus Status { get; set; } // Approved / Rejected
        public string? ReviewerComment { get; set; }
        public int ApprovedBy { get; set; }
    }
}
