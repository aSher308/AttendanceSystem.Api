using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace AttendanceSystem.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;

        public StatisticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetSummaryAsync(int userId, DateTime from, DateTime to)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            var attendances = await _context.Attendances
                .Where(a => a.UserId == userId && a.CheckIn.Date >= from.Date && a.CheckIn.Date <= to.Date)
                .ToListAsync();

            var workingDays = attendances.Count(a => a.Status == AttendanceStatus.OnTime || a.Status == AttendanceStatus.Late || a.Status == AttendanceStatus.LeaveEarly);
            var absentDays = attendances.Count(a => a.Status == AttendanceStatus.Absent);
            var totalHours = attendances.Sum(a => a.CheckOut.HasValue ? (a.CheckOut.Value - a.CheckIn).TotalHours : 0);
            int totalDays = (to - from).Days + 1;
            double absentRate = totalDays > 0 ? (double)absentDays / totalDays : 0;

            return new
            {
                user.Id,
                user.FullName,
                workingDays,
                absentDays,
                absentRate = Math.Round(absentRate * 100, 2),
                totalHours = Math.Round(totalHours, 2)
            };
        }

        public async Task<object> GetLeaveAndOvertimeAsync(int userId, DateTime from, DateTime to)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            var leaves = await _context.LeaveRequests
                .Where(l => l.UserId == userId && l.Status == RequestStatus.Approved &&
                            l.FromDate <= to && l.ToDate >= from)
                .ToListAsync();

            var overtimes = await _context.OvertimeRequests
                .Where(o => o.UserId == userId && o.Status == RequestStatus.Approved && o.Date >= from && o.Date <= to)
                .ToListAsync();

            var leaveDays = leaves.Sum(l => (l.ToDate - l.FromDate).Days + 1);
            var overtimeHours = overtimes.Sum(o => (o.EndTime - o.StartTime).TotalHours);

            return new
            {
                leaveDays,
                overtimeHours = Math.Round(overtimeHours, 2)
            };
        }

        public async Task<byte[]> ExportToExcelAsync(DateTime from, DateTime to)
        {
            var users = await _context.Users.ToListAsync();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("AttendanceStats");

            sheet.Cells[1, 1].Value = "Họ tên";
            sheet.Cells[1, 2].Value = "Ngày làm việc";
            sheet.Cells[1, 3].Value = "Ngày nghỉ";
            sheet.Cells[1, 4].Value = "Vắng mặt";
            sheet.Cells[1, 5].Value = "Tỉ lệ vắng (%)";
            sheet.Cells[1, 6].Value = "Tổng giờ làm";
            sheet.Cells[1, 7].Value = "Giờ tăng ca";

            int row = 2;
            foreach (var user in users)
            {
                var att = await _context.Attendances
                    .Where(a => a.UserId == user.Id && a.CheckIn.Date >= from && a.CheckIn.Date <= to)
                    .ToListAsync();

                var leaves = await _context.LeaveRequests
                    .Where(l => l.UserId == user.Id && l.Status == RequestStatus.Approved && l.FromDate <= to && l.ToDate >= from)
                    .ToListAsync();

                var overtimes = await _context.OvertimeRequests
                    .Where(o => o.UserId == user.Id && o.Status == RequestStatus.Approved && o.Date >= from && o.Date <= to)
                    .ToListAsync();

                int workingDays = att.Count(a => a.Status == AttendanceStatus.OnTime || a.Status == AttendanceStatus.Late || a.Status == AttendanceStatus.LeaveEarly);
                int absentDays = att.Count(a => a.Status == AttendanceStatus.Absent);
                int leaveDays = leaves.Sum(l => (l.ToDate - l.FromDate).Days + 1);
                double overtimeHours = overtimes.Sum(o => (o.EndTime - o.StartTime).TotalHours);
                double totalHours = att.Sum(a => a.CheckOut.HasValue ? (a.CheckOut.Value - a.CheckIn).TotalHours : 0);
                int totalDays = (to - from).Days + 1;
                double absentRate = totalDays > 0 ? (double)absentDays / totalDays * 100 : 0;

                sheet.Cells[row, 1].Value = user.FullName;
                sheet.Cells[row, 2].Value = workingDays;
                sheet.Cells[row, 3].Value = leaveDays;
                sheet.Cells[row, 4].Value = absentDays;
                sheet.Cells[row, 5].Value = Math.Round(absentRate, 2);
                sheet.Cells[row, 6].Value = Math.Round(totalHours, 2);
                sheet.Cells[row, 7].Value = Math.Round(overtimeHours, 2);
                row++;
            }

            return package.GetAsByteArray();
        }

        public async Task<List<object>> GetAllSummaryAsync(DateTime from, DateTime to)
        {
            var users = await _context.Users.ToListAsync();
            var results = new List<object>();

            foreach (var user in users)
            {
                var att = await _context.Attendances
                    .Where(a => a.UserId == user.Id && a.CheckIn.Date >= from && a.CheckIn.Date <= to)
                    .ToListAsync();

                var leaves = await _context.LeaveRequests
                    .Where(l => l.UserId == user.Id && l.Status == RequestStatus.Approved && l.FromDate <= to && l.ToDate >= from)
                    .ToListAsync();

                var overtimes = await _context.OvertimeRequests
                    .Where(o => o.UserId == user.Id && o.Status == RequestStatus.Approved && o.Date >= from && o.Date <= to)
                    .ToListAsync();

                int workingDays = att.Count(a => a.Status == AttendanceStatus.OnTime || a.Status == AttendanceStatus.Late || a.Status == AttendanceStatus.LeaveEarly);
                int absentDays = att.Count(a => a.Status == AttendanceStatus.Absent);
                int leaveDays = leaves.Sum(l => (l.ToDate - l.FromDate).Days + 1);
                double overtimeHours = overtimes.Sum(o => (o.EndTime - o.StartTime).TotalHours);
                double totalHours = att.Sum(a => a.CheckOut.HasValue ? (a.CheckOut.Value - a.CheckIn).TotalHours : 0);
                int totalDays = (to - from).Days + 1;
                double absentRate = totalDays > 0 ? (double)absentDays / totalDays * 100 : 0;

                results.Add(new
                {
                    user.Id,
                    user.FullName,
                    workingDays,
                    leaveDays,
                    absentDays,
                    absentRate = Math.Round(absentRate, 2),
                    totalHours = Math.Round(totalHours, 2),
                    overtimeHours = Math.Round(overtimeHours, 2)
                });
            }

            return results;
        }
    }
}
