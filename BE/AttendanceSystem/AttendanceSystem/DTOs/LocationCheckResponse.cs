namespace AttendanceSystem.DTOs
{
    public class LocationCheckResponse
    {
        public bool IsValid { get; set; }
        public int? LocationId { get; set; }
        public string? LocationName { get; set; }
        public double? RadiusInMeters { get; set; }
    }
}
