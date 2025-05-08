using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot be longer than 100 characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(255, ErrorMessage = "Email cannot be longer than 255 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string PasswordHash { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string PhoneNumber { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Leave balance must be a positive number.")]
        public int LeaveBalance { get; set; } = 12;

        public bool IsActive { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        // Foreign key và navigation property cho Department
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<Attendance> Attendances { get; set; }
        public ICollection<LeaveRequest> LeaveRequests { get; set; }
        public ICollection<OvertimeRequest> OvertimeRequests { get; set; }
        public ICollection<QRCodeEntry> QRCodeEntries { get; set; }
    }
}
