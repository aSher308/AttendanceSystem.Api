using AttendanceSystem.DTOs;

namespace AttendanceSystem.Services.Interfaces
{
    public interface IWorkScheduleService
    {
        Task<List<WorkScheduleResponse>> GetAllAsync(int? id, int? userId, DateTime? fromDate, DateTime? toDate);
        Task<WorkScheduleResponse> CreateAsync(WorkScheduleCreateRequest request);
        Task<bool> UpdateAsync(WorkScheduleUpdateRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
