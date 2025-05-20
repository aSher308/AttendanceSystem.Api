using AttendanceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // Lấy tất cả thông báo cho user
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserNotifications(int userId)
    {
        var notifications = await _notificationService.GetNotificationsForUserAsync(userId);
        return Ok(notifications);
    }

    // Lấy thông báo chưa đọc cho user
    [HttpGet("user/{userId}/unread")]
    public async Task<IActionResult> GetUnreadNotifications(int userId)
    {
        var notifications = await _notificationService.GetUnreadNotificationsForUserAsync(userId);
        return Ok(notifications);
    }

    // Lấy thông báo theo Id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotificationById(int id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null)
            return NotFound();
        return Ok(notification);
    }

    // Tạo thông báo mới (thường backend gọi, ít khi frontend tạo trực tiếp)
    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] SystemNotification notification)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdNotification = await _notificationService.CreateNotificationAsync(notification);
        return CreatedAtAction(nameof(GetNotificationById), new { id = createdNotification.Id }, createdNotification);
    }

    // Đánh dấu thông báo đã đọc
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    // Xóa thông báo
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        await _notificationService.DeleteNotificationAsync(id);
        return NoContent();
    }
}
