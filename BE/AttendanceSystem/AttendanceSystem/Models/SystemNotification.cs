using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class SystemNotification
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255, ErrorMessage = "Title cannot be longer than 255 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(500, ErrorMessage = "Content cannot be longer than 500 characters.")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? UserId { get; set; }

        public bool IsRead { get; set; }

        [Required(ErrorMessage = "Notification type is required.")]
        public NotificationType NotificationType { get; set; }

        public User User { get; set; }
    }
}
