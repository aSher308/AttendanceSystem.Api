using AttendanceSystem.Data;
using AttendanceSystem.DTOs;
using AttendanceSystem.Interfaces;
using AttendanceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogsController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;
        private readonly AppDbContext _context;

        public ActivityLogsController(
            IActivityLogService activityLogService,
            AppDbContext context)
        {
            _activityLogService = activityLogService;
            _context = context;
        }

        // GET: api/activitylogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActivityLogDTO>>> GetActivityLogs(
            [FromQuery] int? userId,
            [FromQuery] string? action,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var query = _context.ActivityLogs
                .Include(l => l.User)
                .AsQueryable();

            // Filter by user
            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId.Value);

            // Filter by action
            if (!string.IsNullOrEmpty(action))
                query = query.Where(l => l.Action.Contains(action));

            // Filter by date range
            if (fromDate.HasValue)
                query = query.Where(l => l.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.Timestamp <= toDate.Value);

            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new ActivityLogDTO
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = l.User.FullName,
                    Action = l.Action,
                    Description = l.Description,
                    Timestamp = l.Timestamp,
                    IPAddress = l.IPAddress,
                    DeviceInfo = l.DeviceInfo
                })
                .ToListAsync();

            return Ok(logs);
        }


        // DELETE: api/activitylogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivityLog(int id)
        {
            var log = await _context.ActivityLogs.FindAsync(id);
            if (log == null)
            {
                return NotFound();
            }

            _context.ActivityLogs.Remove(log);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}