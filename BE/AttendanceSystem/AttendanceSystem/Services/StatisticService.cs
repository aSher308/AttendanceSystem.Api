using AttendanceSystem.Data;
using AttendanceSystem.DTOs;
using AttendanceSystem.Models;
using AttendanceSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceSystem.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly AppDbContext _context;
        private readonly IAttendanceService _attendanceService;
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly IOvertimeRequestService _overtimeRequestService;
        private readonly StatisticConfig _config;

        public StatisticService(
            AppDbContext context,
            IAttendanceService attendanceService,
            ILeaveRequestService leaveRequestService,
            IOvertimeRequestService overtimeRequestService,
            StatisticConfig config = null)
        {
            _context = context;
            _attendanceService = attendanceService;
            _leaveRequestService = leaveRequestService;
            _overtimeRequestService = overtimeRequestService;
            _config = config ?? new StatisticConfig(); // Default config
        }

        public async Task<StatisticDTO> GetStatisticsAsync(int? userId, DateTime fromDate, DateTime toDate)
        {
            var statistics = new StatisticDTO
            {
                FromDate = fromDate,
                ToDate = toDate,
                UserId = userId
            };

            // Get attendances with related schedule and shift info
            var attendances = await _context.Attendances
                .Include(a => a.User)
                .Include(a => a.WorkSchedule)
                .ThenInclude(ws => ws.Shift)
                .Where(a =>
                    (userId == null || a.UserId == userId) &&
                    a.CheckIn.Date >= fromDate.Date &&
                    a.CheckIn.Date <= toDate.Date)
                .ToListAsync();

            // Basic statistics
            statistics.TotalWorkDays = attendances.Count(a => a.CheckOut.HasValue);
            statistics.TotalLateDays = attendances.Count(a => a.Status == AttendanceStatus.Late);
            statistics.TotalLeaveEarlyDays = attendances.Count(a => a.Status == AttendanceStatus.LeaveEarly);
            statistics.TotalAbsentDays = attendances.Count(a => a.Status == AttendanceStatus.Absent);

            // Calculate working hours and penalties
            double totalWorkingHours = 0;
            double totalPenaltyHours = 0;

            foreach (var a in attendances.Where(a => a.CheckOut.HasValue && a.WorkSchedule?.Shift != null))
            {
                var (workedHours, penaltyHours) = CalculateWorkingHoursWithPenaltyDetail(a);
                totalWorkingHours += workedHours;
                totalPenaltyHours += penaltyHours;
            }

            statistics.TotalWorkingHours = totalWorkingHours;
            statistics.TotalPenaltyHours = totalPenaltyHours;
            // Leave statistics
            var leaves = await _leaveRequestService.GetAllAsync(userId, fromDate, toDate, "Approved");
            statistics.TotalLeaveDays = leaves.Sum(l => (l.ToDate - l.FromDate).Days + 1);

            // Overtime statistics
            var overtimes = await _overtimeRequestService.GetAllAsync(userId, fromDate, toDate, "Approved");
            statistics.TotalOvertimeHours = overtimes.Sum(o => (o.EndTime - o.StartTime).TotalHours);

            // User information if single user
            if (userId.HasValue)
            {
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null)
                {
                    statistics.UserName = user.FullName;
                    statistics.CurrentLeaveBalance = user.LeaveBalance;
                }
            }

            return statistics;
        }

        private (double workedHours, double penaltyHours) CalculateWorkingHoursWithPenaltyDetail(Attendance a)
        {
            var shift = a.WorkSchedule.Shift;
            var shiftStart = shift.StartTime;
            var shiftEnd = shift.EndTime;
            var actualStart = a.CheckIn.TimeOfDay;
            var actualEnd = a.CheckOut.Value.TimeOfDay;

            // Xử lý đi muộn
            var startTime = actualStart > shiftStart.Add(TimeSpan.FromMinutes(15))
                ? actualStart : shiftStart;

            // Tính giờ làm lý tưởng (nếu không về sớm)
            var fullShiftHours = (shiftEnd - startTime).TotalHours;
            double workedHours = fullShiftHours;
            double penaltyHours = 0;

            // Xử lý về sớm
            if (actualEnd < shiftEnd)
            {
                var earlyLeaveMinutes = (shiftEnd - actualEnd).TotalMinutes;

                // Nếu về sớm quá ngưỡng
                if (earlyLeaveMinutes > _config.EarlyLeaveThresholdMinutes)
                {
                    // Tính số giờ bị phạt
                    penaltyHours = fullShiftHours * (1 - _config.EarlyLeavePenaltyRate);
                    workedHours = fullShiftHours - penaltyHours;
                }
                else
                {
                    // Tính giờ thực tế nếu về sớm ít
                    workedHours = (actualEnd - startTime).TotalHours;
                }
            }

            return (workedHours, penaltyHours);
        }

        public async Task<byte[]> ExportStatisticsToExcelAsync(int? userId, DateTime fromDate, DateTime toDate)
        {
            var statistics = await GetStatisticsAsync(userId, fromDate, toDate);

            // Get detailed data
            var attendances = await _context.Attendances
                .Include(a => a.User)
                .Include(a => a.WorkSchedule)
                .ThenInclude(ws => ws.Shift)
                .Where(a =>
                    (userId == null || a.UserId == userId) &&
                    a.CheckIn.Date >= fromDate.Date &&
                    a.CheckIn.Date <= toDate.Date)
                .ToListAsync();

            var leaves = await _leaveRequestService.GetAllAsync(userId, fromDate, toDate, null);
            var overtimes = await _overtimeRequestService.GetAllAsync(userId, fromDate, toDate, null);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            // Summary sheet
            var summarySheet = package.Workbook.Worksheets.Add("Tổng hợp");
            summarySheet.Cells[1, 1].Value = "Thống kê từ ngày";
            summarySheet.Cells[1, 2].Value = statistics.FromDate.ToString("dd/MM/yyyy");
            summarySheet.Cells[2, 1].Value = "Đến ngày";
            summarySheet.Cells[2, 2].Value = statistics.ToDate.ToString("dd/MM/yyyy");

            if (userId.HasValue)
            {
                summarySheet.Cells[3, 1].Value = "Nhân viên";
                summarySheet.Cells[3, 2].Value = statistics.UserName;
            }

            // Format header
            using (var range = summarySheet.Cells[5, 1, 14, 2])
            {
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            summarySheet.Cells[5, 1].Value = "Tổng số ngày làm việc";
            summarySheet.Cells[5, 2].Value = statistics.TotalWorkDays;
            summarySheet.Cells[6, 1].Value = "Tổng giờ làm (đã tính phạt)";
            summarySheet.Cells[6, 2].Value = Math.Round(statistics.TotalWorkingHours, 2);
            summarySheet.Cells[7, 1].Value = "Tổng số ngày đi muộn";
            summarySheet.Cells[7, 2].Value = statistics.TotalLateDays;
            summarySheet.Cells[8, 1].Value = "Tổng số ngày về sớm";
            summarySheet.Cells[8, 2].Value = statistics.TotalLeaveEarlyDays;
            summarySheet.Cells[9, 1].Value = "Tổng số ngày vắng mặt";
            summarySheet.Cells[9, 2].Value = statistics.TotalAbsentDays;
            summarySheet.Cells[10, 1].Value = "Tổng số ngày nghỉ phép";
            summarySheet.Cells[10, 2].Value = statistics.TotalLeaveDays;
            summarySheet.Cells[11, 1].Value = "Tổng số giờ tăng ca";
            summarySheet.Cells[11, 2].Value = statistics.TotalOvertimeHours;
            summarySheet.Cells[13, 1].Value = "Tổng giờ bị phạt";
            summarySheet.Cells[13, 2].Value = Math.Round(statistics.TotalPenaltyHours, 2);
            summarySheet.Cells[13, 2].Style.Font.Color.SetColor(Color.Red);

            if (userId.HasValue)
            {
                summarySheet.Cells[12, 1].Value = "Số ngày phép còn lại";
                summarySheet.Cells[12, 2].Value = statistics.CurrentLeaveBalance;
            }

            // Attendance details sheet
            var attendanceSheet = package.Workbook.Worksheets.Add("Chi tiết chấm công");

            // Header
            var headers = new string[] { "Ngày", "Ca làm", "Giờ bắt đầu ca", "Giờ kết thúc ca",
                                       "Check-in", "Check-out", "Giờ làm được tính",
                                       "Trạng thái", "Ghi chú" };
            for (int i = 0; i < headers.Length; i++)
            {
                attendanceSheet.Cells[1, i + 1].Value = headers[i];
                attendanceSheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Data
            for (int i = 0; i < attendances.Count; i++)
            {
                var a = attendances[i];
                var row = i + 2;

                // Cột 1-6 giữ nguyên
                attendanceSheet.Cells[row, 1].Value = a.CheckIn.ToString("dd/MM/yyyy");
                attendanceSheet.Cells[row, 2].Value = a.WorkSchedule?.Shift?.Name ?? "";

                if (a.WorkSchedule?.Shift != null)
                {
                    attendanceSheet.Cells[row, 3].Value = a.WorkSchedule.Shift.StartTime.ToString(@"hh\:mm");
                    attendanceSheet.Cells[row, 4].Value = a.WorkSchedule.Shift.EndTime.ToString(@"hh\:mm");
                }

                attendanceSheet.Cells[row, 5].Value = a.CheckIn.ToString("HH:mm");
                attendanceSheet.Cells[row, 6].Value = a.CheckOut?.ToString("HH:mm") ?? "";

                // Tính toán chi tiết giờ làm và phạt
                if (a.CheckOut.HasValue && a.WorkSchedule?.Shift != null)
                {
                    var (workedHours, penaltyHours) = CalculateWorkingHoursWithPenaltyDetail(a);
                    var shiftHours = (a.WorkSchedule.Shift.EndTime - a.WorkSchedule.Shift.StartTime).TotalHours;
                    var actualHours = (a.CheckOut.Value - a.CheckIn).TotalHours;

                    // Cột 7: Giờ làm được tính (đã trừ phạt)
                    attendanceSheet.Cells[row, 7].Value = Math.Round(workedHours, 2);

                    // Cột 8: Trạng thái
                    attendanceSheet.Cells[row, 8].Value = a.Status.ToString();

                    // Cột 9: Thông tin phạt/ghi chú
                    var note = "";

                    if (a.Status == AttendanceStatus.LeaveEarly)
                    {
                        var earlyMinutes = Math.Round((a.WorkSchedule.Shift.EndTime - a.CheckOut.Value.TimeOfDay).TotalMinutes);
                        note = $"Về sớm {earlyMinutes} phút";

                        if (penaltyHours > 0)
                        {
                            note += $", Phạt: {Math.Round(penaltyHours, 2)} giờ";
                            attendanceSheet.Cells[row, 7].Style.Font.Color.SetColor(Color.Red);
                            attendanceSheet.Cells[row, 9].Style.Font.Color.SetColor(Color.Red);
                        }
                        else
                        {
                            note += " (không bị phạt)";
                        }
                    }
                    else if (a.Status == AttendanceStatus.Late)
                    {
                        var lateMinutes = Math.Round((a.CheckIn.TimeOfDay - a.WorkSchedule.Shift.StartTime).TotalMinutes);
                        note = $"Đi muộn {lateMinutes} phút";
                    }

                    attendanceSheet.Cells[row, 9].Value = note;

                    // Cột 10: Giờ làm lý tưởng (theo ca)
                    attendanceSheet.Cells[row, 10].Value = Math.Round(shiftHours, 2);

                    // Cột 11: Giờ làm thực tế
                    attendanceSheet.Cells[row, 11].Value = Math.Round(actualHours, 2);
                }
                else
                {
                    attendanceSheet.Cells[row, 8].Value = a.Status.ToString();
                }
            }

            // Đặt header cho các cột mới
            attendanceSheet.Cells[1, 10].Value = "Giờ ca làm";
            attendanceSheet.Cells[1, 11].Value = "Giờ thực tế";

            // Auto-fit columns
            attendanceSheet.Cells[attendanceSheet.Dimension.Address].AutoFitColumns();

            // Leave and overtime sheets (similar to previous implementation)
            // ...

            return package.GetAsByteArray();
        }
    }

    public class StatisticConfig
    {
        public int EarlyLeaveThresholdMinutes { get; set; } = 30; // Ngưỡng về sớm để áp dụng phạt
        public double EarlyLeavePenaltyRate { get; set; } = 0.75; // Mức phạt 25% giờ làm
    }
}