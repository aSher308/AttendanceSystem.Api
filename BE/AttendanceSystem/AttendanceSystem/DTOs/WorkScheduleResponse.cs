namespace AttendanceSystem.DTOs
{
    public class WorkScheduleResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int ShiftId { get; set; }
        public string ShiftName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime WorkDate { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; }
    }
}
