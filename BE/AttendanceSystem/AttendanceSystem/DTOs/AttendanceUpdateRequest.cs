using AttendanceSystem.Models;

namespace AttendanceSystem.DTOs
{
    public class AttendanceUpdateRequest
    {
        public int Id { get; set; }
        public DateTime? NewCheckIn { get; set; }
        public DateTime? NewCheckOut { get; set; }
        public string AdjustmentReason { get; set; }
        public int AdjustedBy { get; set; }
        public AttendanceStatus? NewStatus { get; set; }
    }
}
