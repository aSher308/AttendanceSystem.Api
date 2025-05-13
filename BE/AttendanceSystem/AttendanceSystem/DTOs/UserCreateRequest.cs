using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.DTOs
{
    public class UserCreateRequest //Add User mới
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public int? DepartmentId { get; set; }
        public bool IsActive { get; set; } = true;
        public int? RoleId { get; set; }
    }
}
