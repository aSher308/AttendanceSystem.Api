using AttendanceSystem.Data;
using AttendanceSystem.DTOs;
using AttendanceSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace AttendanceSystem.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly AppDbContext _context;

        public ActivityLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ActivityLogDto>> GetLogsAsync(DateTime? from, DateTime? to, int? userId)
        {
            var query = _context.ActivityLogs
                .Include(a => a.User)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(a => a.Timestamp >= from.Value);
            if (to.HasValue)
                query = query.Where(a => a.Timestamp <= to.Value);
            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new ActivityLogDto
                {
                    UserId = a.UserId,
                    UserName = a.User.FullName,
                    Action = a.Action,
                    DeviceInfo = a.DeviceInfo
                })
                .ToListAsync();
        }

        public async Task<byte[]> ExportLogsToExcelAsync(DateTime? from, DateTime? to, int? userId)
        {
            var logs = await GetLogsAsync(from, to, userId);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("ActivityLogs");

            sheet.Cells[1, 1].Value = "User ID";
            sheet.Cells[1, 2].Value = "Họ tên";
            sheet.Cells[1, 3].Value = "Hành động";
            sheet.Cells[1, 4].Value = "Thiết bị";

            int row = 2;
            foreach (var log in logs)
            {
                sheet.Cells[row, 1].Value = log.UserId;
                sheet.Cells[row, 2].Value = log.UserName;
                sheet.Cells[row, 3].Value = log.Action;
                sheet.Cells[row, 4].Value = log.DeviceInfo;
                row++;
            }

            return package.GetAsByteArray();
        }
    }

}
