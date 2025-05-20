namespace AttendanceSystem.DTOs
{
    public class LocationCreateRequest
    {
        public string Name { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusInMeters { get; set; }
        public bool IsDefault { get; set; }
    }
}
