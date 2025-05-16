namespace AttendanceSystem.DTOs
{
    public class AttendanceResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string Status { get; set; }
        public string AttendanceType { get; set; }
        public string LocationName { get; set; }
        public string DeviceInfo { get; set; }
        public string CheckInPhotoUrl { get; set; }
        public string? CheckOutPhotoUrl { get; set; }
        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }
        public string? AdjustmentReason { get; set; }
        public string? AdjustedByName { get; set; }
        public DateTime? AdjustedAt { get; set; }
    }
}
