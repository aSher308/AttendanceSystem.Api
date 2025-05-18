namespace AttendanceSystem.DTOs
{
    public class OvertimeRequestCreateRequest
    {
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Reason { get; set; }
    }
}
