using System.Collections.Generic;
using System.Threading.Tasks;
using AttendanceSystem.Models;

public interface INotificationService
{
    Task<IEnumerable<SystemNotification>> GetNotificationsForUserAsync(int userId);
    Task<IEnumerable<SystemNotification>> GetUnreadNotificationsForUserAsync(int userId);
    Task<SystemNotification> GetNotificationByIdAsync(int id);
    Task<SystemNotification> CreateNotificationAsync(SystemNotification notification);
    Task MarkAsReadAsync(int notificationId);
    Task DeleteNotificationAsync(int notificationId);

    // Các helper tạo notification theo loại
    Task<SystemNotification> CreateSystemNotification(string title, string content, int? userId = null);
    Task<SystemNotification> CreateReminderNotification(string title, string content, int userId);
    Task<SystemNotification> CreateApprovalNotification(string title, string content, int userId);
}
