using AttendanceSystem.DTOs;
using AttendanceSystem.Models;
using AttendanceSystem.Data;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Services.Interfaces;

namespace AttendanceSystem.Services
{
    public class WorkScheduleService : IWorkScheduleService
    {
        private readonly AppDbContext _context;

        public WorkScheduleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkScheduleResponse>> GetAllAsync(int? id, int? userId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.WorkSchedules
                .Include(ws => ws.User)
                .Include(ws => ws.Shift)
                .AsQueryable();

            if (id.HasValue)
                query = query.Where(ws => ws.Id == id);

            if (userId.HasValue)
                query = query.Where(ws => ws.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(ws => ws.WorkDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(ws => ws.WorkDate <= toDate.Value);

            return await query.Select(ws => new WorkScheduleResponse
            {
                Id = ws.Id,
                UserId = ws.UserId,
                FullName = ws.User.FullName,
                ShiftId = ws.ShiftId,
                ShiftName = ws.Shift.Name,
                StartTime = ws.Shift.StartTime,
                EndTime = ws.Shift.EndTime,
                WorkDate = ws.WorkDate.AddHours(7),
                Note = ws.Note,
                Status = ws.Status
            }).ToListAsync();
        }

        public async Task<WorkScheduleResponse> CreateAsync(WorkScheduleCreateRequest request)
        {
            var schedule = new WorkSchedule
            {
                UserId = request.UserId,
                ShiftId = request.ShiftId,
                WorkDate = request.WorkDate.Date,
                Note = request.Note,
                Status = request.Status
            };

            _context.WorkSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(schedule.UserId);
            var shift = await _context.Shifts.FindAsync(schedule.ShiftId);

            return new WorkScheduleResponse
            {
                Id = schedule.Id,
                UserId = schedule.UserId,
                FullName = user?.FullName ?? "",
                ShiftId = schedule.ShiftId,
                ShiftName = shift?.Name ?? "",
                StartTime = shift?.StartTime ?? TimeSpan.Zero,
                EndTime = shift?.EndTime ?? TimeSpan.Zero,
                WorkDate = schedule.WorkDate.AddHours(7),
                Note = schedule.Note,
                Status = schedule.Status
            };
        }

        public async Task<bool> UpdateAsync(WorkScheduleUpdateRequest request)
        {
            var schedule = await _context.WorkSchedules.FindAsync(request.Id);
            if (schedule == null) return false;

            schedule.ShiftId = request.ShiftId;
            schedule.WorkDate = request.WorkDate.Date;
            schedule.Note = request.Note;
            schedule.Status = request.Status;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var schedule = await _context.WorkSchedules.FindAsync(id);
            if (schedule == null) return false;
            _context.WorkSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
