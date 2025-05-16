using AttendanceSystem.Models;

namespace AttendanceSystem.DTOs
{
    public class AttendanceCheckInRequest
    {
        public int UserId { get; set; }
        public string LocationName { get; set; }
        public string DeviceInfo { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? PhotoUrl { get; set; }
        public AttendanceType AttendanceType { get; set; }
    }
}
