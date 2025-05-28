using AttendanceSystem.DTOs;
using AttendanceSystem.Interfaces;
using AttendanceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemNotificationsController : ControllerBase
    {
        private readonly ISystemNotificationService _notificationService;

        public SystemNotificationsController(ISystemNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]

        public async Task<ActionResult<SystemNotificationDTO>> CreateNotification([FromBody] CreateSystemNotificationDTO dto)
        {
            var notification = await _notificationService.CreateNotificationAsync(dto);
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<SystemNotificationDTO>>> GetUserNotifications(int userId)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("user/{userId}/unread")]
        public async Task<ActionResult<IEnumerable<SystemNotificationDTO>>> GetUnreadNotifications(int userId)
        {
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

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateNotificationStatus(int id, [FromBody] UpdateNotificationStatusDTO dto)
        {
            await _notificationService.UpdateNotificationStatusAsync(id, dto);
            return NoContent();
        }

        [HttpPost("user/{userId}/mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            return NoContent();
        }

        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteNotification(int id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            return NoContent();
        }

        [HttpGet("filter")]
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