// Services/ActivityLogService.cs
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Models.DTOs;
using AttendanceSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceSystem.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly AppDbContext _context;

        public ActivityLogService(AppDbContext context)
        {
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task LogActivityAsync(int userId, string action, string description, string ipAddress, string deviceInfo)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Action = Truncate(action, 100),
                Description = Truncate(description, 500),
                Timestamp = DateTime.UtcNow,
                IPAddress = Truncate(ipAddress, 50),
                DeviceInfo = Truncate(deviceInfo, 200)
            };

            await _context.ActivityLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<ActivityLogDto>> GetPagedLogsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? userId = null)
        {
            var query = _context.ActivityLogs
                .Include(x => x.User)
                .AsQueryable();

            if (fromDate.HasValue) query = query.Where(x => x.Timestamp >= fromDate);
            if (toDate.HasValue) query = query.Where(x => x.Timestamp <= toDate);
            if (userId.HasValue) query = query.Where(x => x.UserId == userId);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ActivityLogDto
                {
                    Id = x.Id,
                    Timestamp = x.Timestamp,
                    UserName = x.User.FullName,
                    Action = x.Action,
                    Description = x.Description,
                    IPAddress = x.IPAddress,
                    DeviceInfo = x.DeviceInfo
                })
                .ToListAsync();

            return new PagedResult<ActivityLogDto>
            {
                Items = logs,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<byte[]> ExportExcelAsync(DateTime? fromDate, DateTime? toDate)
        {
            var logs = await _context.ActivityLogs
                .Include(x => x.User)
                .Where(x => (!fromDate.HasValue || x.Timestamp >= fromDate) &&
                           (!toDate.HasValue || x.Timestamp <= toDate))
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Activity Logs");

            // Header
            worksheet.Cells["A1:F1"].Style.Font.Bold = true;
            worksheet.Cells["A1:F1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells["A1:F1"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            worksheet.Cells[1, 1].Value = "Thời gian";
            worksheet.Cells[1, 2].Value = "Người dùng";
            worksheet.Cells[1, 3].Value = "Hành động";
            worksheet.Cells[1, 4].Value = "Mô tả";
            worksheet.Cells[1, 5].Value = "IP";
            worksheet.Cells[1, 6].Value = "Thiết bị";

            // Data
            for (int i = 0; i < logs.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = logs[i].Timestamp.ToString("g");
                worksheet.Cells[i + 2, 2].Value = logs[i].User?.FullName;
                worksheet.Cells[i + 2, 3].Value = logs[i].Action;
                worksheet.Cells[i + 2, 4].Value = logs[i].Description;
                worksheet.Cells[i + 2, 5].Value = logs[i].IPAddress;
                worksheet.Cells[i + 2, 6].Value = logs[i].DeviceInfo;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            using var stream = new MemoryStream();
            package.SaveAs(stream);
            return stream.ToArray();
        }

        private static string Truncate(string value, int maxLength) =>
            string.IsNullOrEmpty(value) ? value : value.Length <= maxLength ? value : value[..maxLength];
    }
}