using AttendanceSystem.Data;
using AttendanceSystem.DTOs;
using AttendanceSystem.Interfaces;
using AttendanceSystem.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace AttendanceSystem.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly AppDbContext _context;

        public ActivityLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogActivityAsync(CreateActivityLogDTO logDto)
        {
            try
            {
                var log = new ActivityLog
                {
                    UserId = logDto.UserId,
                    Action = logDto.Action,
                    Description = logDto.Description,
                    IPAddress = logDto.IPAddress,
                    DeviceInfo = logDto.DeviceInfo,
                    Timestamp = VietnamTimeHelper.Now
                };

                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ghi log hoạt động: {ex.Message}");
            }
        }
        public async Task<int> DeleteOldLogsAsync(int weeksToKeep)
        {
            var cutoffDate = VietnamTimeHelper.Now.AddDays(-7 * weeksToKeep);

            var oldLogs = await _context.ActivityLogs
                .Where(l => l.Timestamp < cutoffDate)
                .ToListAsync();

            _context.ActivityLogs.RemoveRange(oldLogs);
            return await _context.SaveChangesAsync();
        }

        public async Task<byte[]> ExportActivityLogsToExcelAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var logs = await _context.ActivityLogs
                .Include(l => l.User) // Nếu có navigation đến User
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Activity Logs");

                // Header
                worksheet.Cells[1, 1].Value = "User ID";
                worksheet.Cells[1, 2].Value = "User Name";
                worksheet.Cells[1, 3].Value = "Action";
                worksheet.Cells[1, 4].Value = "Description";
                worksheet.Cells[1, 5].Value = "IP Address";
                worksheet.Cells[1, 6].Value = "Device Info";
                worksheet.Cells[1, 7].Value = "Timestamp";

                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                // Dữ liệu
                for (int i = 0; i < logs.Count; i++)
                {
                    var row = i + 2;
                    var log = logs[i];

                    worksheet.Cells[row, 1].Value = log.UserId;
                    worksheet.Cells[row, 2].Value = log.User?.FullName ?? "N/A"; // cần navigation property
                    worksheet.Cells[row, 3].Value = log.Action;
                    worksheet.Cells[row, 4].Value = log.Description;
                    worksheet.Cells[row, 5].Value = log.IPAddress;
                    worksheet.Cells[row, 6].Value = log.DeviceInfo;
                    worksheet.Cells[row, 7].Value = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                }

                worksheet.Cells.AutoFitColumns();

                return await Task.FromResult(package.GetAsByteArray());
            }
        }
    }
}