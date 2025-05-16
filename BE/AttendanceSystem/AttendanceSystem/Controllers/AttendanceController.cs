using AttendanceSystem.DTOs;
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] AttendanceCheckInRequest request)
        {
            var result = await _attendanceService.CheckInAsync(request);
            return Ok(result);
        }

        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut([FromBody] AttendanceCheckOutRequest request)
        {
            var result = await _attendanceService.CheckOutAsync(request);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string? status)
        {
            var list = await _attendanceService.GetAllAsync(userId, fromDate, toDate, status);
            return Ok(list);
        }

/*        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _attendanceService.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }*/

        [HttpPut("adjust")]
        public async Task<IActionResult> Update([FromBody] AttendanceUpdateRequest request)
        {
            var result = await _attendanceService.UpdateAsync(request);
            return result ? Ok("Cập nhật chấm công thành công") : NotFound();
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] int? userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var file = await _attendanceService.ExportToExcelAsync(userId, fromDate, toDate);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "attendance.xlsx");
        }
    }
}
