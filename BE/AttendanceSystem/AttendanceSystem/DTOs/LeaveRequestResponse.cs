namespace AttendanceSystem.DTOs
{
    public class LeaveRequestResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string LeaveType { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewerComment { get; set; }
    }
}
