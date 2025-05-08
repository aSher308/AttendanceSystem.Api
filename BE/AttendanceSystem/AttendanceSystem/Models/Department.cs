using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Department name is required.")]
        [StringLength(100, ErrorMessage = "Department name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
