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
            var today = GetVietnamTime().Date;

            var existing = await _context.Attendances.FirstOrDefaultAsync(a => a.UserId == request.UserId && a.CheckIn.Date == today);
            if (existing != null) throw new AppException("Đã check-in trong ngày hôm nay");

            var schedule = await _context.WorkSchedules.Include(ws => ws.Shift)
                                     .FirstOrDefaultAsync(ws => ws.UserId == request.UserId && ws.WorkDate == today);
            if (schedule == null) throw new AppException("Hôm nay bạn không có lịch làm việc");

            var location = await _context.Locations.FirstOrDefaultAsync(l => l.Name == request.LocationName);
            if (location == null) throw new AppException("Không tìm thấy địa điểm hợp lệ để check-in");

            // Kiểm tra khoảng cách với vị trí hợp lệ
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                double distance = CalculateDistance(request.Latitude.Value, request.Longitude.Value, location.Latitude, location.Longitude);
                if (distance > location.RadiusInMeters)
                {
                    throw new AppException("Bạn đang ở ngoài phạm vi cho phép để check-in (" + Math.Round(distance) + "m)");
                }
            }

            var vietnamNow = GetVietnamTime();
            var expectedCheckIn = today.Add(schedule.Shift.StartTime);
            var diffCheckIn = (vietnamNow - expectedCheckIn).TotalMinutes;
            var status = diffCheckIn > 15 ? AttendanceStatus.Late : AttendanceStatus.OnTime;

            var attendance = new Attendance
            {
                UserId = request.UserId,
                CheckIn = vietnamNow,
                Status = status,
                AttendanceType = request.AttendanceType,
                LocationName = request.LocationName,
                LocationId = location.Id,
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
            var today = GetVietnamTime().Date;

            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.UserId == request.UserId && a.CheckIn.Date == today);
            if (attendance == null) throw new AppException("Chưa check-in trong ngày hôm nay");
            if (attendance.CheckOut.HasValue) throw new AppException("Đã check-out trước đó");

            var schedule = await _context.WorkSchedules.Include(ws => ws.Shift)
                                    .FirstOrDefaultAsync(ws => ws.UserId == request.UserId && ws.WorkDate == today);
            if (schedule == null) throw new AppException("Hôm nay bạn không có lịch làm việc");

            var location = await _context.Locations.FirstOrDefaultAsync(l => l.Name == attendance.LocationName);
            if (location == null) throw new AppException("Không tìm thấy địa điểm hợp lệ để check-out");

            // Kiểm tra khoảng cách vị trí check-out
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                double distance = CalculateDistance(request.Latitude.Value, request.Longitude.Value, location.Latitude, location.Longitude);
                if (distance > location.RadiusInMeters)
                {
                    throw new AppException("Bạn đang ở ngoài phạm vi cho phép để check-out (" + Math.Round(distance) + "m)");
                }
            }

            var vietnamNow = GetVietnamTime();
            var expectedCheckOut = today.Add(schedule.Shift.EndTime);
            var diffCheckOut = (expectedCheckOut - vietnamNow).TotalMinutes;

            if (diffCheckOut > 15)
            {
                attendance.Status = AttendanceStatus.LeaveEarly;
            }

            attendance.CheckOut = vietnamNow;
            attendance.CheckOutLatitude = request.Latitude;
            attendance.CheckOutLongitude = request.Longitude;
            attendance.CheckOutPhotoUrl = request.PhotoUrl;
            attendance.DeviceInfo = request.DeviceInfo;

            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(request.UserId);
            return ToResponse(attendance, user);
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
                "0 23 * * *" // mỗi ngày lúc 23:00 UTC
            );
        }

        public async Task<List<AttendanceResponse>> GetAllAsync(int? userId, DateTime? fromDate, DateTime? toDate, string? status)
        {
            var query = _context.Attendances.Include(a => a.User).AsQueryable();

            if (userId.HasValue) query = query.Where(a => a.UserId == userId);
            if (fromDate.HasValue) query = query.Where(a => a.CheckIn.Date >= fromDate.Value.Date);
            if (toDate.HasValue) query = query.Where(a => a.CheckIn.Date <= toDate.Value.Date);
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AttendanceStatus>(status, out var s))
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
    }
}
