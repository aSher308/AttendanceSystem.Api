using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(50, ErrorMessage = "Role name cannot be longer than 50 characters.")]
        public string Name { get; set; }

        [StringLength(255, ErrorMessage = "Description cannot be longer than 255 characters.")]
        public string Description { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}
