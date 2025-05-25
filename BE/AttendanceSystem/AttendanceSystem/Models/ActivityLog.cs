// Models/ActivityLog.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystem.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [StringLength(50)]
        public string IPAddress { get; set; }

        [StringLength(200)]
        public string DeviceInfo { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}