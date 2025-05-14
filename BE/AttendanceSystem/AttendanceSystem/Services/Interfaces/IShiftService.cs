using AttendanceSystem.DTOs;
using AttendanceSystem.Models;

namespace AttendanceSystem.Services
{
    public interface IShiftService
    {
        Task<List<ShiftResponse>> GetAllAsync();
        Task<ShiftResponse?> GetByIdAsync(int id);
        Task<ShiftResponse> CreateAsync(ShiftCreateRequest request);
        Task<bool> UpdateAsync(ShiftUpdateRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
