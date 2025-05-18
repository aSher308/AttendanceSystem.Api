using AttendanceSystem.DTOs;
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;

        public LeaveRequestController(ILeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
        }
        // Tạo đơn nghỉ
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LeaveRequestCreateRequest request)
        {
            var result = await _leaveRequestService.CreateAsync(request);
            return Ok(result);
        }
        // Thay đổi thông tin đơn nghỉ
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] LeaveRequestUpdateRequest request)
        {
            var success = await _leaveRequestService.UpdateAsync(request);
            if (!success) return BadRequest("Không thể cập nhật đơn nghỉ.");
            return Ok("Cập nhật thành công");
        }
        // Xóa đơn nghỉ
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _leaveRequestService.DeleteAsync(id);
            if (!success) return NotFound("Không tìm thấy hoặc không thể xoá đơn nghỉ.");
            return Ok("Xoá thành công");
        }
        // Lấy tất cả các đơn
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status)
        {
            var list = await _leaveRequestService.GetAllAsync(userId, from, to, status);
            return Ok(list);
        }

        /*        [HttpGet("{id}")]
                public async Task<IActionResult> GetById(int id)
                {
                    var item = await _leaveRequestService.GetByIdAsync(id);
                    if (item == null) return NotFound();
                    return Ok(item);
                }*/
        // Chấp nhận hoặc hủy đơn
        [HttpPut("approve")]
        public async Task<IActionResult> Approve([FromBody] LeaveRequestApprovalRequest request)
        {
            var success = await _leaveRequestService.ApproveAsync(request);
            if (!success) return BadRequest("Không thể duyệt đơn nghỉ.");
            return Ok("Đã xử lý đơn nghỉ.");
        }
    }
}
