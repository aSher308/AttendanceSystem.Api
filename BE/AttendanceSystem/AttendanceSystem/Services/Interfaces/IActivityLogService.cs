// Services/Interfaces/IActivityLogService.cs
using AttendanceSystem.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace AttendanceSystem.Services.Interfaces
{
    public interface IActivityLogService
    {
        Task LogActivityAsync(int userId, string action, string description, string ipAddress, string deviceInfo);
        Task<PagedResult<ActivityLogDto>> GetPagedLogsAsync(int pageNumber = 1, int pageSize = 10, DateTime? fromDate = null, DateTime? toDate = null, int? userId = null);
        Task<byte[]> ExportExcelAsync(DateTime? fromDate, DateTime? toDate);
    }
}