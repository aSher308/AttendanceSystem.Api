namespace AttendanceSystem.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusInMeters { get; set; }
        public bool IsDefault { get; set; }
    }
}
