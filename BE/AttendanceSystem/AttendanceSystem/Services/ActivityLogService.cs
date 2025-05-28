using AttendanceSystem.Data;
using AttendanceSystem.DTOs;
using AttendanceSystem.Interfaces;
using AttendanceSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
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
                    Timestamp = DateTime.UtcNow
                };

                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ghi log hoạt động: {ex.Message}");
            }
        }
    }
}