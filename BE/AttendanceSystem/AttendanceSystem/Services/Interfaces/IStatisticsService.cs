using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttendanceSystem.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<object> GetSummaryAsync(int userId, DateTime from, DateTime to);
        Task<object> GetLeaveAndOvertimeAsync(int userId, DateTime from, DateTime to);
        Task<byte[]> ExportToExcelAsync(DateTime from, DateTime to);
        Task<List<object>> GetAllSummaryAsync(DateTime from, DateTime to);
    }
}
