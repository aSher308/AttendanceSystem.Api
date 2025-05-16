using AttendanceSystem.DTOs;

namespace AttendanceSystem.Services.Interfaces
{
    public interface ILeaveRequestService
    {
        Task<LeaveRequestResponse> CreateAsync(LeaveRequestCreateRequest request);
        Task<bool> UpdateAsync(LeaveRequestUpdateRequest request);
        Task<bool> DeleteAsync(int id);
        Task<List<LeaveRequestResponse>> GetAllAsync(int? userId, DateTime? from, DateTime? to, string? status);
        Task<LeaveRequestResponse?> GetByIdAsync(int id);
        Task<bool> ApproveAsync(LeaveRequestApprovalRequest request);
        Task<bool> CheckLeaveBalanceEnoughAsync(int userId, DateTime from, DateTime to);
    }
}
