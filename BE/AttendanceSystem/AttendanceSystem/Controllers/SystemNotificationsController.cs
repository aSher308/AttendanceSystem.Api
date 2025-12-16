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

        // Tạo thông báo (Chỉ Admin mới được tạo thủ công)
        [HttpPost]
        [RequireRole("Admin")]
        public async Task<ActionResult<SystemNotificationDTO>> CreateNotification([FromBody] CreateSystemNotificationDTO dto)
        {
            var notification = await _notificationService.CreateNotificationAsync(dto);
            // Vì hàm Create trả về DTO (có thể là dummy nếu gửi all), nên check ID để trả về 201 hoặc 200
            if (notification.Id == 0)
                return Ok(notification);

            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }

        // Xem thông báo của User đang đăng nhập
        [HttpGet("me")]
        [RequireRole("User", "Admin")] // Cho phép cả User và Admin
        public async Task<ActionResult<IEnumerable<SystemNotificationDTO>>> GetMyNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId.Value);
            return Ok(notifications);
        }

        // Xem chi tiết 1 thông báo (Chỉ Admin)
        [HttpGet("{id}")]
        [RequireRole("Admin")]
        public async Task<ActionResult<SystemNotificationDTO>> GetNotification(int id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null) return NotFound();
            return Ok(notification);
        }

        // Cập nhật trạng thái (Đã đọc/Chưa đọc) cho 1 thông báo cụ thể
        [HttpPut("{id}/status")]
        [RequireRole("User", "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateNotificationStatusDTO dto)
        {
            // Logic kiểm tra quyền sở hữu (Optional: Bạn có thể thêm check xem thông báo id này có phải của user này không)
            await _notificationService.UpdateNotificationStatusAsync(id, dto);
            return NoContent();
        }

/*        // Đánh dấu tất cả đã đọc (QUAN TRỌNG: Đã sửa bảo mật)
        [HttpPost("user/{userId}/mark-all-read")]
        [RequireRole("User", "Admin")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return Unauthorized();

            // Lấy quyền Admin
            var isAdmin = await _accountService.IsAdminAsync(currentUserId.Value);

            // Nếu không phải Admin VÀ đang cố đánh dấu cho người khác -> Chặn
            if (!isAdmin && currentUserId.Value != userId)
            {
                return Forbid(); // 403 Forbidden
            }

            await _notificationService.MarkAllAsReadAsync(userId);
            return NoContent();
        }*/

        //Tự động đánh dấu đã đọc 
        [HttpPost("me/mark-all-read")]
        [RequireRole("User", "Admin")]
        public async Task<IActionResult> MarkMyNotificationsAsRead()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(currentUserId.Value);
            return NoContent();
        }

        // Xóa thông báo (Chỉ Admin)
        [HttpDelete("{id}")]
        [RequireRole("Admin")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            return NoContent();
        }

        // Filter (Chỉ Admin)
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