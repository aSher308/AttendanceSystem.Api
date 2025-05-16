using AttendanceSystem.DTOs;

namespace AttendanceSystem.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<AttendanceResponse> CheckInAsync(AttendanceCheckInRequest request);
        Task<AttendanceResponse> CheckOutAsync(AttendanceCheckOutRequest request);
        Task<List<AttendanceResponse>> GetAllAsync(int? userId, DateTime? fromDate, DateTime? toDate, string? status);
        Task<AttendanceResponse?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(AttendanceUpdateRequest request);
        Task<byte[]> ExportToExcelAsync(int? userId, DateTime? fromDate, DateTime? toDate);
        Task AutoMarkAbsentAsync(DateTime date);
    }
}
