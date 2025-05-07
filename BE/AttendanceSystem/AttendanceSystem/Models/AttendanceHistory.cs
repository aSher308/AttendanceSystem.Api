namespace AttendanceSystem.Models
{
    public class AttendanceHistory
    {
        public int Id { get; set; }

        public int AttendanceId { get; set; }
        public Attendance Attendance { get; set; }

        public DateTime ChangeDate { get; set; }
        public int ChangedBy { get; set; } // Người thực hiện thay đổi (ví dụ: Admin)
        public User ChangedByUser { get; set; } // Quan hệ đến User (người thay đổi)
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }

        public string Notes { get; set; } // Ghi chú thay đổi
    }
}