using AttendanceSystem.Models;

namespace AttendanceSystem.DTOs
{
    public class LeaveRequestUpdateRequest
    {
        public int Id { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public LeaveType LeaveType { get; set; }
        public string Reason { get; set; }
    }

}
