using AttendanceSystem.Models;

namespace AttendanceSystem.DTOs
{
    public class OvertimeApprovalRequest
    {
        public int Id { get; set; }
        public RequestStatus Status { get; set; }
        public int ApprovedBy { get; set; }
    }
}
