namespace AttendanceSystem.DTOs
{
    public class WorkScheduleCreateRequest
    {
        public int UserId { get; set; }
        public int ShiftId { get; set; }
        public DateTime WorkDate { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; }
    }

}
