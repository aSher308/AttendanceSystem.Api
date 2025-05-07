using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Check-in time is required.")]
        public DateTime CheckIn { get; set; }

        public DateTime? CheckOut { get; set; }

        [Required(ErrorMessage = "Attendance status is required.")]
        public AttendanceStatus Status { get; set; }

        [Required(ErrorMessage = "Attendance type is required.")]
        public AttendanceType AttendanceType { get; set; }

        [StringLength(100, ErrorMessage = "Location name cannot be longer than 100 characters.")]
        public string LocationName { get; set; }

        [StringLength(500, ErrorMessage = "Device info cannot be longer than 500 characters.")]
        public string DeviceInfo { get; set; }

        // GPS
        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }

        // Ảnh khuôn mặt
        [StringLength(255)]
        public string CheckInPhotoUrl { get; set; }

        [StringLength(255)]
        public string CheckOutPhotoUrl { get; set; }

        // Nhật ký chỉnh sửa
        public int? AdjustedBy { get; set; }
        public DateTime? AdjustedAt { get; set; }

        [StringLength(255)]
        public string AdjustmentReason { get; set; }

        // Quan hệ
        public User User { get; set; }

        // Kiểm tra nếu trạng thái cần phải điều chỉnh sau khi CheckIn
        public bool IsCheckedOut => CheckOut.HasValue;
        public bool IsCheckedIn => CheckIn != default(DateTime);
    }
}
