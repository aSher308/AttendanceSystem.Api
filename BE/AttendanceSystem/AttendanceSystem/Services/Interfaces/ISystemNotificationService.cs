using AttendanceSystem.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttendanceSystem.Interfaces
{
    public interface ISystemNotificationService
    {
        Task<SystemNotificationDTO> CreateNotificationAsync(CreateSystemNotificationDTO notificationDto);
        Task<IEnumerable<SystemNotificationDTO>> GetUserNotificationsAsync(int userId);
        Task<IEnumerable<SystemNotificationDTO>> GetUnreadNotificationsAsync(int userId);
        Task<SystemNotificationDTO> GetNotificationByIdAsync(int id);
        Task UpdateNotificationStatusAsync(int id, UpdateNotificationStatusDTO statusDto);
        Task DeleteNotificationAsync(int id);
        Task MarkAllAsReadAsync(int userId);
        Task<IEnumerable<SystemNotificationDTO>> FilterNotificationsAsync(NotificationFilterDTO filter);
    }
}