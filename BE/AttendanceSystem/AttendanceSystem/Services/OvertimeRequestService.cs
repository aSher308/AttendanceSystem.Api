using AttendanceSystem.Data;
using AttendanceSystem.DTOs;
using AttendanceSystem.Interfaces;
using AttendanceSystem.Models;
using AttendanceSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Services
{
    public class OvertimeRequestService : IOvertimeRequestService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ISystemNotificationService _notificationService; // Inject thêm

        public OvertimeRequestService(
            AppDbContext context,
            IEmailService emailService,
            ISystemNotificationService notificationService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<OvertimeRequestResponse> CreateAsync(OvertimeRequestCreateRequest request)
        {
            var overtime = new OvertimeRequest
            {
                UserId = request.UserId,
                Date = request.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Reason = request.Reason,
                Status = RequestStatus.Pending
            };

            _context.OvertimeRequests.Add(overtime);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(request.UserId);

            // Lấy danh sách Admin
            var adminUsers = await _context.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.User)
                .Where(ur => ur.Role.Name == "Admin")
                .Select(ur => ur.User)
                .Distinct()
                .ToListAsync();

            foreach (var admin in adminUsers)
            {
                // Email
                await _emailService.SendEmailAsync(admin.Email, "Yêu cầu tăng ca mới",
                    $"{user.FullName} vừa gửi yêu cầu tăng ca ngày {request.Date:dd/MM/yyyy} từ {request.StartTime} đến {request.EndTime}.\nLý do: {request.Reason}");

                // Notification
                await _notificationService.CreateNotificationAsync(new CreateSystemNotificationDTO
                {
                    Title = "Yêu cầu tăng ca mới",
                    Content = $"{user.FullName} xin tăng ca ngày {request.Date:dd/MM}",
                    UserId = admin.Id,
                    NotificationType = NotificationType.Approval
                });
            }

            return await GetByIdAsync(overtime.Id) ?? throw new Exception("Không lấy được thông tin tăng ca vừa tạo.");
        }

        public async Task<bool> ApproveAsync(OvertimeApprovalRequest request)
        {
            var overtime = await _context.OvertimeRequests.FindAsync(request.Id);
            if (overtime == null || overtime.Status != RequestStatus.Pending) return false;

            overtime.Status = request.Status;
            overtime.ApprovedBy = request.ApprovedBy;
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(overtime.UserId);

            string body = request.Status == RequestStatus.Approved
                ? $"Yêu cầu tăng ca ngày {overtime.Date:dd/MM/yyyy} từ {overtime.StartTime} đến {overtime.EndTime} đã được duyệt."
                : $"Yêu cầu tăng ca của bạn đã bị từ chối.";

            // Email
            await _emailService.SendEmailAsync(user.Email, "Kết quả tăng ca", body);

            // Notification
            await _notificationService.CreateNotificationAsync(new CreateSystemNotificationDTO
            {
                Title = "Kết quả duyệt tăng ca",
                Content = request.Status == RequestStatus.Approved
                    ? $"Yêu cầu tăng ca ngày {overtime.Date:dd/MM} đã được DUYỆT."
                    : "Yêu cầu tăng ca của bạn đã bị TỪ CHỐI.",
                UserId = user.Id,
                NotificationType = NotificationType.System
            });

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var overtime = await _context.OvertimeRequests.FindAsync(id);
            if (overtime == null || overtime.Status != RequestStatus.Pending) return false;

            _context.OvertimeRequests.Remove(overtime);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OvertimeRequestResponse?> GetByIdAsync(int id)
        {
            var overtime = await _context.OvertimeRequests.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
            if (overtime == null) return null;

            return new OvertimeRequestResponse
            {
                Id = overtime.Id,
                UserId = overtime.UserId,
                FullName = overtime.User.FullName,
                Date = overtime.Date,
                StartTime = overtime.StartTime,
                EndTime = overtime.EndTime,
                Reason = overtime.Reason,
                Status = overtime.Status.ToString(),
                ApprovedByName = overtime.ApprovedBy.HasValue ? _context.Users.Find(overtime.ApprovedBy)?.FullName : null
            };
        }

        public async Task<List<OvertimeRequestResponse>> GetAllAsync(int? userId, DateTime? from, DateTime? to, string? status)
        {
            var query = _context.OvertimeRequests.Include(o => o.User).AsQueryable();

            if (userId.HasValue) query = query.Where(o => o.UserId == userId);
            if (from.HasValue) query = query.Where(o => o.Date >= from);
            if (to.HasValue) query = query.Where(o => o.Date <= to);
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RequestStatus>(status, out var s))
                query = query.Where(o => o.Status == s);

            var list = await query.ToListAsync();
            return list.Select(o => new OvertimeRequestResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                FullName = o.User.FullName,
                Date = o.Date,
                StartTime = o.StartTime,
                EndTime = o.EndTime,
                Reason = o.Reason,
                Status = o.Status.ToString(),
                ApprovedByName = o.ApprovedBy.HasValue ? _context.Users.Find(o.ApprovedBy)?.FullName : null
            }).ToList();
        }
    }
}