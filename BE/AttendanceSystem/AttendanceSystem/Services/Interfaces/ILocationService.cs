using AttendanceSystem.DTOs;
using AttendanceSystem.Models;

namespace AttendanceSystem.Services.Interfaces
{
    public interface ILocationService
    {
        Task<List<Location>> GetAllAsync();
        Task<Location?> GetByIdAsync(int id);
        Task<Location> CreateAsync(LocationCreateRequest request);
        Task<bool> UpdateAsync(LocationUpdateRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
