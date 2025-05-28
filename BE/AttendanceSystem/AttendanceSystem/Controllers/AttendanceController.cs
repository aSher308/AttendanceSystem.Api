using AttendanceSystem.Attributes;
using AttendanceSystem.DTOs;
using AttendanceSystem.Services;
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireRole("User", "Admin")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IAccountService _accountService;
        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }
        // Check-in
        [HttpPost("check-in")]
        [RequireRole("User")]
        public async Task<IActionResult> CheckIn([FromBody] AttendanceCheckInRequest request)
        {
            var result = await _attendanceService.CheckInAsync(request);
            return Ok(result);
        }
        // Check-out
        [HttpPost("check-out")]
        [RequireRole("User")]
        public async Task<IActionResult> CheckOut([FromBody] AttendanceCheckOutRequest request)
        {
            var result = await _attendanceService.CheckOutAsync(request);
            return Ok(result);
        }
        // Get all Attendance
        [HttpGet]
        [RequireRole("User", "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int? userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string? status)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return Unauthorized();

            var isAdmin = await _accountService.IsAdminAsync(currentUserId.Value);
            if (!isAdmin)
            {
                userId = currentUserId.Value; // Ép userId về chính mình
            }

            var list = await _attendanceService.GetAllAsync(userId, fromDate, toDate, status);
            return Ok(list);
        }


        /*        [HttpGet("{id}")]
                public async Task<IActionResult> GetById(int id)
                {
                    var result = await _attendanceService.GetByIdAsync(id);
                    return result == null ? NotFound() : Ok(result);
                }*/

        // Update Attendance
        [HttpPut("adjust")]
        [RequireRole("Admin")]
        public async Task<IActionResult> Update([FromBody] AttendanceUpdateRequest request)
        {
            var result = await _attendanceService.UpdateAsync(request);
            return result ? Ok("Cập nhật chấm công thành công") : NotFound();
        }
        //Xuất file Excel
        [HttpGet("export")]
        [RequireRole("Admin")]
        public async Task<IActionResult> Export([FromQuery] int? userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var file = await _attendanceService.ExportToExcelAsync(userId, fromDate, toDate);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "attendance.xlsx");
        }
    }
}
