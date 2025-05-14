using AttendanceSystem.DTOs;
using AttendanceSystem.Services;
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftController : ControllerBase
    {
        private readonly IShiftService _shiftService;

        public ShiftController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var shifts = await _shiftService.GetAllAsync();
            return Ok(shifts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var shift = await _shiftService.GetByIdAsync(id);
            return shift == null ? NotFound() : Ok(shift);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ShiftCreateRequest request)
        {
            var created = await _shiftService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ShiftUpdateRequest request)
        {
            var result = await _shiftService.UpdateAsync(request);
            return result ? Ok("Cập nhật shift thành công") : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _shiftService.DeleteAsync(id);
            return result ? Ok("Xóa shift thành công") : NotFound();
        }
    }
}