using AttendanceSystem.DTOs;
using System.Threading.Tasks;

namespace AttendanceSystem.Interfaces
{
    public interface IActivityLogService
    {
        Task LogActivityAsync(CreateActivityLogDTO logDto);
    }
}