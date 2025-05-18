using AttendanceSystem.DTOs;

namespace AttendanceSystem.Services.Interfaces
{
    public interface IOvertimeRequestService
    {
        Task<OvertimeRequestResponse> CreateAsync(OvertimeRequestCreateRequest request);
        Task<bool> ApproveAsync(OvertimeApprovalRequest request);
        Task<bool> DeleteAsync(int id);
        Task<OvertimeRequestResponse?> GetByIdAsync(int id);
        Task<List<OvertimeRequestResponse>> GetAllAsync(int? userId, DateTime? from, DateTime? to, string? status);
    }
}
