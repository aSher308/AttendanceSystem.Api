using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class QRCodeEntry
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Scan time is required.")]
        public DateTime ScanTime { get; set; }

        [StringLength(500, ErrorMessage = "QRCode data cannot be longer than 500 characters.")]
        public string QRCodeData { get; set; }

        [Required(ErrorMessage = "Scan type is required.")]
        public ScanType ScanType { get; set; }

        [StringLength(500, ErrorMessage = "Device info cannot be longer than 500 characters.")]
        public string DeviceInfo { get; set; }

        public User User { get; set; }
    }
}
