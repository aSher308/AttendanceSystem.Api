using AttendanceSystem.DTOs;
using AttendanceSystem.Models;
using AttendanceSystem.Data;
using AttendanceSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Hangfire;
using AttendanceSystem.Exceptions;

namespace AttendanceSystem.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;
        public AttendanceService(AppDbContext context)
        {
            _context = context;
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Radius of Earth in meters
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static DateTime GetVietnamTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        }

        public async Task<AttendanceResponse> CheckInAsync(AttendanceCheckInRequest request)
        {
            var now = GetVietnamTime();
            var today = now.Date;

            var schedulesToday = await _context.WorkSchedules
                .Include(ws => ws.Shift)
                .Where(ws => ws.UserId == request.UserId && ws.WorkDate == today)
                .ToListAsync();

            if (!schedulesToday.Any())
                throw new AppException("Hôm nay bạn không có lịch làm việc");

            var checkedInScheduleIds = await _context.Attendances
                .Where(a => a.UserId == request.UserId && a.CheckIn.Date == today)
                .Select(a => a.WorkScheduleId)
                .ToListAsync();

            var matchedSchedule = schedulesToday.FirstOrDefault(s =>
            {
                var checkInTime = today.Add(s.Shift.StartTime);
                var diff = (now - checkInTime).TotalMinutes;
                return !checkedInScheduleIds.Contains(s.Id) && diff >= -180 && diff <= 180;
            });

            if (matchedSchedule == null)
                throw new AppException("Không tìm thấy ca làm phù hợp hoặc đã check-in tất cả ca hôm nay");

            var location = await FindMatchingLocationAsync(request.Latitude, request.Longitude);
            if (location == null)
                throw new AppException("Không ở trong phạm vi địa điểm nào được phép check-in");

            var status = (now - today.Add(matchedSchedule.Shift.StartTime)).TotalMinutes > 15
                ? AttendanceStatus.Late
                : AttendanceStatus.OnTime;

            var attendance = new Attendance
            {
                UserId = request.UserId,
                WorkScheduleId = matchedSchedule.Id,
                CheckIn = now,
                Status = status,
                AttendanceType = request.AttendanceType,
                LocationId = location.Id,
                LocationName = location.Name,
                DeviceInfo = request.DeviceInfo,
                CheckInLatitude = request.Latitude,
                CheckInLongitude = request.Longitude,
                CheckInPhotoUrl = request.PhotoUrl
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(request.UserId);
            return ToResponse(attendance, user);
        }

        public async Task<AttendanceResponse> CheckOutAsync(AttendanceCheckOutRequest request)
        {
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.WorkScheduleId == request.WorkScheduleId && a.CheckOut == null)
                ?? throw new AppException("Không tìm thấy bản ghi check-in chưa check-out cho ca này");

            var schedule = await _context.WorkSchedules
                .Include(ws => ws.Shift)
                .FirstOrDefaultAsync(ws => ws.Id == request.WorkScheduleId && ws.UserId == request.UserId)
                ?? throw new AppException("Không tìm thấy ca làm việc tương ứng");

            var now = GetVietnamTime();
            var expectedCheckOut = schedule.WorkDate.Add(schedule.Shift.EndTime);
            var diff = (now - expectedCheckOut).TotalMinutes;

            if (diff < -30)
                throw new AppException("Chưa đến giờ check-out, vui lòng thử lại sau");

            var location = await FindMatchingLocationAsync(request.Latitude, request.Longitude);
            if (location == null)
                throw new AppException("Bạn đang ở ngoài phạm vi cho phép để check-out");

            if (diff < 0)
                attendance.Status = AttendanceStatus.LeaveEarly;

            attendance.CheckOut = now;
            attendance.CheckOutLatitude = request.Latitude;
            attendance.CheckOutLongitude = request.Longitude;
            attendance.CheckOutPhotoUrl = request.PhotoUrl;
            attendance.DeviceInfo = request.DeviceInfo;

            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(request.UserId);
            return ToResponse(attendance, user);
        }
        private async Task<Location?> FindMatchingLocationAsync(double? lat, double? lng)
        {
            if (!lat.HasValue || !lng.HasValue) return null;

            var locations = await _context.Locations.ToListAsync();
            return locations.FirstOrDefault(loc =>
                CalculateDistance(lat.Value, lng.Value, loc.Latitude, loc.Longitude) <= loc.RadiusInMeters);
        }

        public async Task AutoMarkAbsentAsync(DateTime date)
        {
            var schedules = await _context.WorkSchedules
                .Where(ws => ws.WorkDate == date && (ws.Status == null || ws.Status == "Working"))
                .ToListAsync();

            foreach (var schedule in schedules)
            {
                var exists = await _context.Attendances
                    .AnyAsync(a => a.UserId == schedule.UserId && a.CheckIn.Date == date);

                if (!exists)
                {
                    _context.Attendances.Add(new Attendance
                    {
                        UserId = schedule.UserId,
                        CheckIn = date,
                        Status = AttendanceStatus.Absent,
                        AttendanceType = AttendanceType.None,
                        LocationName = "",
                        DeviceInfo = "Tự động đánh dấu vắng",
                        CheckInPhotoUrl = null,
                        CheckInLatitude = null,
                        CheckInLongitude = null
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public static void ScheduleDailyAbsentJob()
        {
            RecurringJob.AddOrUpdate<IAttendanceService>(
                "auto-mark-absent",
                service => service.AutoMarkAbsentAsync(GetVietnamTime().Date),
                "0 23 * * *" // mỗi ngày lúc 23:00 
            );
        }

        public async Task<List<AttendanceResponse>> GetAllAsync(int? userId, DateTime? fromDate, DateTime? toDate, string? status)
        {
            var query = _context.Attendances.Include(a => a.User).AsQueryable();

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(a => a.CheckIn >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(a => a.CheckIn < toDate.Value.Date.AddDays(1));

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AttendanceStatus>(status, true, out var s))
                query = query.Where(a => a.Status == s);

            var list = await query.ToListAsync();

            return list.Select(a => ToResponse(a, a.User)).ToList();
        }

        public async Task<AttendanceResponse?> GetByIdAsync(int id)
        {
            var att = await _context.Attendances.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
            return att == null ? null : ToResponse(att, att.User);
        }

        public async Task<bool> UpdateAsync(AttendanceUpdateRequest request)
        {
            var att = await _context.Attendances.FindAsync(request.Id);
            if (att == null) return false;

            if (request.NewCheckIn.HasValue)
                att.CheckIn = request.NewCheckIn.Value;

            if (request.NewCheckOut.HasValue)
                att.CheckOut = request.NewCheckOut.Value;

            if (request.NewStatus.HasValue)
                att.Status = request.NewStatus.Value;

            att.AdjustmentReason = request.AdjustmentReason;
            att.AdjustedBy = request.AdjustedBy;
            att.AdjustedAt = GetVietnamTime();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<byte[]> ExportToExcelAsync(int? userId, DateTime? fromDate, DateTime? toDate)
        {
            var data = await GetAllAsync(userId, fromDate, toDate, null);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Attendance");

            // Header
            sheet.Cells[1, 1].Value = "Họ tên";
            sheet.Cells[1, 2].Value = "Ngày";
            sheet.Cells[1, 3].Value = "Check-In";
            sheet.Cells[1, 4].Value = "Check-Out";
            sheet.Cells[1, 5].Value = "Trạng thái";
            sheet.Cells[1, 6].Value = "Vị trí";
            sheet.Cells[1, 7].Value = "Loại chấm công";

            for (int i = 0; i < data.Count; i++)
            {
                var d = data[i];
                sheet.Cells[i + 2, 1].Value = d.FullName;
                sheet.Cells[i + 2, 2].Value = d.CheckIn.ToString("yyyy-MM-dd");
                sheet.Cells[i + 2, 3].Value = d.CheckIn.ToString("HH:mm");
                sheet.Cells[i + 2, 4].Value = d.CheckOut?.ToString("HH:mm") ?? "";
                sheet.Cells[i + 2, 5].Value = d.Status;
                sheet.Cells[i + 2, 6].Value = d.LocationName;
                sheet.Cells[i + 2, 7].Value = d.AttendanceType;
            }

            return package.GetAsByteArray();
        }

        private static AttendanceResponse ToResponse(Attendance a, User? user)
        {
            return new AttendanceResponse
            {
                Id = a.Id,
                UserId = a.UserId,
                FullName = user?.FullName ?? "",
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                Status = a.Status.ToString(),
                AttendanceType = a.AttendanceType.ToString(),
                LocationName = a.LocationName,
                DeviceInfo = a.DeviceInfo,
                CheckInPhotoUrl = a.CheckInPhotoUrl,
                CheckOutPhotoUrl = a.CheckOutPhotoUrl,
                CheckInLatitude = a.CheckInLatitude,
                CheckInLongitude = a.CheckInLongitude,
                CheckOutLatitude = a.CheckOutLatitude,
                CheckOutLongitude = a.CheckOutLongitude,
                AdjustmentReason = a.AdjustmentReason,
                AdjustedByName = a.AdjustedBy.HasValue ? user?.FullName : null,
                AdjustedAt = a.AdjustedAt
            };
        }

        public async Task<LocationCheckResponse> CheckLocationAsync(double latitude, double longitude)
        {
            var location = await FindMatchingLocationAsync(latitude, longitude);
            return new LocationCheckResponse
            {
                IsValid = location != null,
                LocationId = location?.Id,
                LocationName = location?.Name,
                RadiusInMeters = location?.RadiusInMeters
            };
        }

    }
}
