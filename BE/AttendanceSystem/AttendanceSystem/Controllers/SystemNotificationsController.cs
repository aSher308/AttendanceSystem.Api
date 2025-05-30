using AttendanceSystem.Attributes;
using AttendanceSystem.DTOs;
using AttendanceSystem.Interfaces;
using AttendanceSystem.Models;
using AttendanceSystem.Services;
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemNotificationsController : ControllerBase
    {
        private readonly ISystemNotificationService _notificationService;
        private readonly IAccountService _accountService;
        public SystemNotificationsController(ISystemNotificationService notificationService, IAccountService accountService)
        {
            _notificationService = notificationService;
            _accountService = accountService;
        }

        //Tạo thông báo
        [HttpPost]
        [RequireRole("Admin")]
        public async Task<ActionResult<SystemNotificationDTO>> CreateNotification([FromBody] CreateSystemNotificationDTO dto)
        {
            var notification = await _notificationService.CreateNotificationAsync(dto);
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }
        // Xem thông báo của User
        [HttpGet("me")]
        [RequireRole("User")]
        public async Task<ActionResult<IEnumerable<SystemNotificationDTO>>> GetUserNotifications(int userId)
        {
            // Lấy userId từ token
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return Unauthorized();

            var isAdmin = await _accountService.IsAdminAsync(currentUserId.Value);
            if (!isAdmin)
            {
                userId = currentUserId.Value; // Ép userId về chính mình
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }
        // Xem thông báo chưa đọc của User
        [HttpGet("me/unread")]
        [RequireRole("User")]
        public async Task<ActionResult<IEnumerable<SystemNotificationDTO>>> GetUnreadNotifications(int userId)
        {
            // Lấy userId từ token
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return Unauthorized();

            var isAdmin = await _accountService.IsAdminAsync(currentUserId.Value);
            if (!isAdmin)
            {
                userId = currentUserId.Value; // Ép userId về chính mình
            }

            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SystemNotificationDTO>> GetNotification(int id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null) return NotFound();
            return Ok(notification);
        }

        //Sửa thông báo
        [HttpPut("{id}/status")]
        [RequireRole("Admin")]
        public async Task<IActionResult> UpdateNotificationStatus(int id, [FromBody] UpdateNotificationStatusDTO dto)
        {
            await _notificationService.UpdateNotificationStatusAsync(id, dto);
            return NoContent();
        }
        // Đánh dấu tất cả đã đọc
        [HttpPost("user/{userId}/mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            return NoContent();
        }

        // Xóa thông báo
        [HttpDelete("{id}")]
        [RequireRole("Admin")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            return NoContent();
        }

        //Xem danh sách thông báo 
        [HttpGet("filter")]
        [RequireRole("Admin")]
        public async Task<ActionResult<IEnumerable<SystemNotificationDTO>>> FilterNotifications(
            [FromQuery] int? userId,
            [FromQuery] bool? isRead,
            [FromQuery] NotificationType? notificationType,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var filter = new NotificationFilterDTO
            {
                UserId = userId,
                IsRead = isRead,
                NotificationType = notificationType,
                FromDate = fromDate,
                ToDate = toDate
            };

            var notifications = await _notificationService.FilterNotificationsAsync(filter);
            return Ok(notifications);
        }
    }
}