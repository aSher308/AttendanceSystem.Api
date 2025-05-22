namespace AttendanceSystem.DTOs
{
    public class ActivityLogDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string DeviceInfo { get; set; }
    }

}
