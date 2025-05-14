using AttendanceSystem.Data;
using AttendanceSystem.DTOs;
using AttendanceSystem.Models;
using AttendanceSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Services
{
    public class ShiftService : IShiftService
    {
        private readonly AppDbContext _context;

        public ShiftService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShiftResponse>> GetAllAsync()
        {
            return await _context.Shifts.Select(s => new ShiftResponse
            {
                Id = s.Id,
                Name = s.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsActive = s.IsActive,
                Description = s.Description
            }).ToListAsync();
        }

        public async Task<ShiftResponse?> GetByIdAsync(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) return null;

            return new ShiftResponse
            {
                Id = shift.Id,
                Name = shift.Name,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                IsActive = shift.IsActive,
                Description = shift.Description
            };
        }

        public async Task<ShiftResponse> CreateAsync(ShiftCreateRequest request)
        {
            var shift = new Shift
            {
                Name = request.Name,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsActive = request.IsActive,
                Description = request.Description
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            return new ShiftResponse
            {
                Id = shift.Id,
                Name = shift.Name,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                IsActive = shift.IsActive,
                Description = shift.Description
            };
        }

        public async Task<bool> UpdateAsync(ShiftUpdateRequest request)
        {
            var shift = await _context.Shifts.FindAsync(request.Id);
            if (shift == null) return false;

            shift.Name = request.Name;
            shift.StartTime = request.StartTime;
            shift.EndTime = request.EndTime;
            shift.IsActive = request.IsActive;
            shift.Description = request.Description;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) return false;
            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
