using AttendanceSystem.DTOs;

namespace AttendanceSystem.Services.Interfaces
{
    public interface IActivityLogService
    {
        Task<List<ActivityLogDto>> GetLogsAsync(DateTime? from, DateTime? to, int? userId);
        Task<byte[]> ExportLogsToExcelAsync(DateTime? from, DateTime? to, int? userId);
    }

}
