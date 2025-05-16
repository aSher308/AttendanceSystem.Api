using AttendanceSystem.DTOs;
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkScheduleController : ControllerBase
    {
        private readonly IWorkScheduleService _workScheduleService;

        public WorkScheduleController(IWorkScheduleService workScheduleService)
        {
            _workScheduleService = workScheduleService;
        }
        // Lấy tất cả lịch làm
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? id, [FromQuery] int? userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var result = await _workScheduleService.GetAllAsync(id, userId, fromDate, toDate);
            return Ok(result);
        }
        // Tạo lịch làm
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkScheduleCreateRequest request)
        {
            var created = await _workScheduleService.CreateAsync(request);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }
        // Sửa lịch làm
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] WorkScheduleUpdateRequest request)
        {
            var result = await _workScheduleService.UpdateAsync(request);
            return result ? Ok("Cập nhật lịch làm thành công") : NotFound();
        }
        // Xóa lịch làm
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _workScheduleService.DeleteAsync(id);
            return result ? Ok("Xóa lịch làm thành công") : NotFound();
        }
    }
}
