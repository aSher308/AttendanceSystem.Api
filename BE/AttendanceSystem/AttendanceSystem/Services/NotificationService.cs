using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Models;
using AttendanceSystem.Data;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SystemNotification>> GetNotificationsForUserAsync(int userId)
    {
        return await _context.SystemNotifications
            .Where(n => n.UserId == null || n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemNotification>> GetUnreadNotificationsForUserAsync(int userId)
    {
        return await _context.SystemNotifications
            .Where(n => (n.UserId == null || n.UserId == userId) && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<SystemNotification> GetNotificationByIdAsync(int id)
    {
        return await _context.SystemNotifications.FindAsync(id);
    }

    public async Task<SystemNotification> CreateNotificationAsync(SystemNotification notification)
    {
        notification.CreatedAt = VietnamTimeHelper.Now;
        notification.IsRead = false;

        _context.SystemNotifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.SystemNotifications.FindAsync(notificationId);
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteNotificationAsync(int notificationId)
    {
        var notification = await _context.SystemNotifications.FindAsync(notificationId);
        if (notification != null)
        {
            _context.SystemNotifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    // Helper methods for creating notifications by type

    public Task<SystemNotification> CreateSystemNotification(string title, string content, int? userId = null)
    {
        return CreateNotificationAsync(new SystemNotification
        {
            Title = title,
            Content = content,
            UserId = userId,
            NotificationType = NotificationType.System
        });
    }

    public Task<SystemNotification> CreateReminderNotification(string title, string content, int userId)
    {
        return CreateNotificationAsync(new SystemNotification
        {
            Title = title,
            Content = content,
            UserId = userId,
            NotificationType = NotificationType.Reminder
        });
    }

    public Task<SystemNotification> CreateApprovalNotification(string title, string content, int userId)
    {
        return CreateNotificationAsync(new SystemNotification
        {
            Title = title,
            Content = content,
            UserId = userId,
            NotificationType = NotificationType.Approval
        });
    }
}
