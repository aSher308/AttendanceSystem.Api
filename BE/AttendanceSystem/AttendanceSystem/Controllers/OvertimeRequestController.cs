using AttendanceSystem.Attributes;
using AttendanceSystem.DTOs;
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireRole("User", "Admin")]
    public class OvertimeRequestController : ControllerBase
    {
        private readonly IOvertimeRequestService _service;

        public OvertimeRequestController(IOvertimeRequestService service)
        {
            _service = service;
        }

        // Tạo yêu cầu tăng ca (user)
        [RequireRole("User", "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OvertimeRequestCreateRequest request)
        {
            var result = await _service.CreateAsync(request);
            return Ok(result);
        }

        // Duyệt/từ chối tăng ca (admin)
        [RequireAdmin]
        [HttpPut("approve")]
        public async Task<IActionResult> Approve([FromBody] OvertimeApprovalRequest request)
        {
            var success = await _service.ApproveAsync(request);
            if (!success) return BadRequest("Không thể duyệt yêu cầu tăng ca.");
            return Ok("Đã xử lý yêu cầu.");
        }

        // Xoá yêu cầu (chưa duyệt)
        [RequireRole("User", "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound("Không tìm thấy hoặc không thể xoá.");
            return Ok("Đã xoá.");
        }

/*        // Lấy theo ID
        [RequireRole("User", "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Ok(data);
        }*/

        // Lọc danh sách
        [RequireRole("User", "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId, DateTime? from, DateTime? to, string? status)
        {
            var data = await _service.GetAllAsync(userId, from, to, status);
            return Ok(data);
        }
    }
}
