namespace AttendanceSystem.DTOs
{
    public class ShiftResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }
}
