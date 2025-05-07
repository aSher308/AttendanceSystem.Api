namespace AttendanceSystem.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; }
        public string DeviceInfo { get; set; }

        public User User { get; set; }
    }
}
