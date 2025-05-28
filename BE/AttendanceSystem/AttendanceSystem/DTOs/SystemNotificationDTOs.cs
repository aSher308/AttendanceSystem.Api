using AttendanceSystem.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.DTOs
{
    public class SystemNotificationDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public bool IsRead { get; set; }
        public NotificationType NotificationType { get; set; }
    }

    public class CreateSystemNotificationDTO
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [StringLength(500, ErrorMessage = "Nội dung không được vượt quá 500 ký tự")]
        public string Content { get; set; }

        public int? UserId { get; set; }
        public NotificationType NotificationType { get; set; } = NotificationType.System;
    }

    public class UpdateNotificationStatusDTO
    {
        public bool IsRead { get; set; }
    }

    public class NotificationFilterDTO
    {
        public int? UserId { get; set; }
        public bool? IsRead { get; set; }
        public NotificationType? NotificationType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}