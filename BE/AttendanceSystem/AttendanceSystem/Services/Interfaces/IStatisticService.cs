using AttendanceSystem.DTOs;
using Hangfire.Storage.Monitoring;
using System.Threading.Tasks;

namespace AttendanceSystem.Services.Interfaces
{
    public interface IStatisticService
    {
        Task<StatisticDTO> GetStatisticsAsync(int? userId, DateTime fromDate, DateTime toDate);
        Task<byte[]> ExportStatisticsToExcelAsync(int? userId, DateTime fromDate, DateTime toDate);
    }
}