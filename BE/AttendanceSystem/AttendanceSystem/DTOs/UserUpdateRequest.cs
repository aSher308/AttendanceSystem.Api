using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.DTOs
{
    public class UserUpdateRequest //Cập nhật User
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public int? DepartmentId { get; set; }
        public bool IsActive { get; set; }
    }
}
