namespace AttendanceSystem.DTOs
{
    public class OvertimeRequestResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string? ApprovedByName { get; set; }
    }
}
