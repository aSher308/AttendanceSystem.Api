using AttendanceSystem.Data;
using AttendanceSystem.DTOs;
using AttendanceSystem.Models;
using AttendanceSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public LeaveRequestService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<LeaveRequestResponse> CreateAsync(LeaveRequestCreateRequest request)
        {
            if (!await CheckLeaveBalanceEnoughAsync(request.UserId, request.FromDate, request.ToDate))
                throw new Exception("Bạn không đủ ngày phép để tạo đơn này.");

            var leave = new LeaveRequest
            {
                UserId = request.UserId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                LeaveType = request.LeaveType,
                Reason = request.Reason,
                Status = RequestStatus.Pending,
                CreatedAt = VietnamTimeHelper.Now,
                CreatedBy = request.CreatedBy
            };

            _context.LeaveRequests.Add(leave);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(request.UserId);
            var adminEmails = await _context.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.User)
                .Where(ur => ur.Role.Name == "Admin")
                .Select(ur => ur.User.Email)
                .Distinct()
                .ToListAsync();

            foreach (var email in adminEmails)
            {
                await _emailService.SendEmailAsync(email, "Đơn nghỉ mới", $"{user.FullName} vừa tạo đơn nghỉ từ {request.FromDate:dd/MM/yyyy} đến {request.ToDate:dd/MM/yyyy}. Lý do: {request.Reason}");
            }

            return await GetByIdAsync(leave.Id) ?? throw new Exception("Không lấy được thông tin đơn nghỉ vừa tạo.");
        }

        public async Task<bool> UpdateAsync(LeaveRequestUpdateRequest request)
        {
            var leave = await _context.LeaveRequests.FindAsync(request.Id);
            if (leave == null || leave.Status != RequestStatus.Pending) return false;

            leave.FromDate = request.FromDate;
            leave.ToDate = request.ToDate;
            leave.LeaveType = request.LeaveType;
            leave.Reason = request.Reason;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave == null || leave.Status != RequestStatus.Pending) return false;

            _context.LeaveRequests.Remove(leave);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<LeaveRequestResponse>> GetAllAsync(int? userId, DateTime? from, DateTime? to, string? status)
        {
            var query = _context.LeaveRequests.Include(l => l.User).AsQueryable();

            if (userId.HasValue) query = query.Where(l => l.UserId == userId);
            if (from.HasValue) query = query.Where(l => l.FromDate >= from);
            if (to.HasValue) query = query.Where(l => l.ToDate <= to);
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RequestStatus>(status, out var s))
                query = query.Where(l => l.Status == s);

            var list = await query.ToListAsync();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<LeaveRequestResponse?> GetByIdAsync(int id)
        {
            var leave = await _context.LeaveRequests.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null) return null;
            return MapToResponse(leave);
        }

        public async Task<bool> ApproveAsync(LeaveRequestApprovalRequest request)
        {
            var leave = await _context.LeaveRequests.FindAsync(request.Id);
            if (leave == null || leave.Status != RequestStatus.Pending) return false;

            leave.Status = request.Status;
            leave.ReviewedAt = VietnamTimeHelper.Now;
            leave.ReviewerComment = request.ReviewerComment;
            leave.ApprovedBy = request.ApprovedBy;

            var user = await _context.Users.FindAsync(leave.UserId);

            if (request.Status == RequestStatus.Approved)
            {
                int totalLeaveDays = (leave.ToDate - leave.FromDate).Days + 1;
                if (user.LeaveBalance < totalLeaveDays)
                    throw new Exception("Người dùng không còn đủ ngày phép để duyệt đơn.");

                user.LeaveBalance -= totalLeaveDays;

                var schedules = await _context.WorkSchedules
                    .Where(ws => ws.UserId == leave.UserId && ws.WorkDate >= leave.FromDate && ws.WorkDate <= leave.ToDate)
                    .ToListAsync();

                foreach (var schedule in schedules)
                {
                    schedule.Status = "Leave";
                    schedule.Note = "Đã duyệt đơn nghỉ phép";
                }
            }

            await _context.SaveChangesAsync();

            string body = request.Status == RequestStatus.Approved
                ? $"Đơn nghỉ của bạn từ {leave.FromDate:dd/MM/yyyy} đến {leave.ToDate:dd/MM/yyyy} đã được duyệt."
                : $"Đơn nghỉ của bạn đã bị từ chối. Ghi chú: {request.ReviewerComment}";

            await _emailService.SendEmailAsync(user.Email, "Kết quả đơn nghỉ phép", body);
            return true;
        }


        public async Task<bool> CheckLeaveBalanceEnoughAsync(int userId, DateTime from, DateTime to)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            int totalLeaveDays = (to - from).Days + 1;
            return user.LeaveBalance >= totalLeaveDays;
        }

        private LeaveRequestResponse MapToResponse(LeaveRequest l)
        {
            var reviewerName = l.ApprovedBy.HasValue
                ? _context.Users.Find(l.ApprovedBy)?.FullName
                : null;

            return new LeaveRequestResponse
            {
                Id = l.Id,
                UserId = l.UserId,
                FullName = l.User.FullName,
                FromDate = l.FromDate,
                ToDate = l.ToDate,
                LeaveType = l.LeaveType.ToString(),
                Reason = l.Reason,
                Status = l.Status.ToString(),
                CreatedAt = l.CreatedAt,
                ReviewedAt = l.ReviewedAt,
                ReviewerName = reviewerName,
                ReviewerComment = l.ReviewerComment
            };
        }
    }
}
