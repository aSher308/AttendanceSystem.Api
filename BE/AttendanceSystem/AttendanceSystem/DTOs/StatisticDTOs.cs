using System;

namespace AttendanceSystem.DTOs
{
    public class StatisticDTO
    {
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int TotalWorkDays { get; set; }
        public double TotalWorkingHours { get; set; }
        public int TotalLateDays { get; set; }
        public int TotalLeaveEarlyDays { get; set; }
        public int TotalAbsentDays { get; set; }
        public int TotalLeaveDays { get; set; }
        public double TotalOvertimeHours { get; set; }
        public int CurrentLeaveBalance { get; set; }

        // Thêm các thông tin phạt nếu cần
        public double TotalPenaltyHours { get; set; }
    }
}