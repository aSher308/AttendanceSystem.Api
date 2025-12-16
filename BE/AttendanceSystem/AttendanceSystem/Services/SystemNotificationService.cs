using AttendanceSystem.Data;
using AttendanceSystem.DTOs;
using AttendanceSystem.Interfaces;
using AttendanceSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceSystem.Services
{
    public class SystemNotificationService : ISystemNotificationService
    {
        private readonly AppDbContext _context;

        public SystemNotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SystemNotificationDTO> CreateNotificationAsync(CreateSystemNotificationDTO notificationDto)
        {
            // Case 1: Gửi thông báo cho 1 người cụ thể
            if (notificationDto.UserId.HasValue)
            {
                var notification = new SystemNotification
                {
                    Title = notificationDto.Title,
                    Content = notificationDto.Content,
                    UserId = notificationDto.UserId,
                    NotificationType = notificationDto.NotificationType,
                    CreatedAt = VietnamTimeHelper.Now,
                    IsRead = false
                };

                _context.SystemNotifications.Add(notification);
                await _context.SaveChangesAsync();

                return await GetNotificationByIdAsync(notification.Id);
            }
            // Case 2: Gửi cho TOÀN BỘ User (UserId là null)
            else
            {
                // 1. Lấy danh sách ID của tất cả User đang hoạt động (IsActive = true)
                var activeUserIds = await _context.Users
                    .Where(u => u.IsActive)
                    .Select(u => u.Id)
                    .ToListAsync();

                if (!activeUserIds.Any())
                {
                    // Nếu không có user nào thì return dummy để không lỗi
                    return new SystemNotificationDTO
                    {
                        Title = notificationDto.Title,
                        Content = "Không có user nào để gửi"
                    };
                }

                // 2. Tạo danh sách thông báo để Insert 1 lần (Bulk Insert)
                var notificationsToInsert = new List<SystemNotification>();
                var now = VietnamTimeHelper.Now;

                foreach (var userId in activeUserIds)
                {
                    notificationsToInsert.Add(new SystemNotification
                    {
                        Title = notificationDto.Title,
                        Content = notificationDto.Content,
                        UserId = userId, // Gán cụ thể cho từng User
                        NotificationType = notificationDto.NotificationType,
                        CreatedAt = now,
                        IsRead = false
                    });
                }

                _context.SystemNotifications.AddRange(notificationsToInsert);
                await _context.SaveChangesAsync();

                // Trả về DTO đại diện (vì tạo nhiều bản ghi nên không trả về 1 ID cụ thể được)
                return new SystemNotificationDTO
                {
                    Id = 0,
                    Title = notificationDto.Title,
                    Content = notificationDto.Content,
                    CreatedAt = now,
                    UserId = null,
                    UserName = "All Users",
                    IsRead = false,
                    NotificationType = notificationDto.NotificationType
                };
            }
        }

        public async Task<IEnumerable<SystemNotificationDTO>> GetUserNotificationsAsync(int userId)
        {
            // Lấy thông báo của user VÀ cả những thông báo chung (UserId = null - nếu có dữ liệu cũ)
            return await _context.SystemNotifications
                .Where(n => n.UserId == userId || n.UserId == null)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => MapToDTO(n))
                .ToListAsync();
        }

        public async Task<IEnumerable<SystemNotificationDTO>> GetUnreadNotificationsAsync(int userId)
        {
            return await _context.SystemNotifications
                .Where(n => (n.UserId == userId || n.UserId == null) && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => MapToDTO(n))
                .ToListAsync();
        }

        public async Task<SystemNotificationDTO> GetNotificationByIdAsync(int id)
        {
            var notification = await _context.SystemNotifications
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == id);

            return notification == null ? null : MapToDTO(notification);
        }

        public async Task UpdateNotificationStatusAsync(int id, UpdateNotificationStatusDTO statusDto)
        {
            var notification = await _context.SystemNotifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = statusDto.IsRead;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteNotificationAsync(int id)
        {
            var notification = await _context.SystemNotifications.FindAsync(id);
            if (notification != null)
            {
                _context.SystemNotifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unreadNotifications = await _context.SystemNotifications
                .Where(n => (n.UserId == userId) && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<SystemNotificationDTO>> FilterNotificationsAsync(NotificationFilterDTO filter)
        {
            var query = _context.SystemNotifications
                .Include(n => n.User)
                .AsQueryable();

            if (filter.UserId.HasValue)
                query = query.Where(n => n.UserId == filter.UserId || n.UserId == null);

            if (filter.IsRead.HasValue)
                query = query.Where(n => n.IsRead == filter.IsRead);

            if (filter.NotificationType.HasValue)
                query = query.Where(n => n.NotificationType == filter.NotificationType);

            if (filter.FromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= filter.FromDate);

            if (filter.ToDate.HasValue)
                query = query.Where(n => n.CreatedAt <= filter.ToDate);

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => MapToDTO(n))
                .ToListAsync();
        }

        private static SystemNotificationDTO MapToDTO(SystemNotification notification)
        {
            return new SystemNotificationDTO
            {
                Id = notification.Id,
                Title = notification.Title,
                Content = notification.Content,
                CreatedAt = notification.CreatedAt,
                UserId = notification.UserId,
                UserName = notification.User?.FullName ?? "System/All",
                IsRead = notification.IsRead,
                NotificationType = notification.NotificationType
            };
        }
    }
}