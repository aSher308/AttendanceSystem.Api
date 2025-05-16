namespace AttendanceSystem.DTOs
{
    public class AttendanceCheckOutRequest
    {
        public int UserId { get; set; }
        public string DeviceInfo { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
