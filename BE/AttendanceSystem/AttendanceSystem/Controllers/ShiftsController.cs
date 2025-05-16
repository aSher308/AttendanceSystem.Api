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

        //Get All Shift
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var shifts = await _shiftService.GetAllAsync();
            return Ok(shifts);
        }

        //Find Shift by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var shift = await _shiftService.GetByIdAsync(id);
            return shift == null ? NotFound() : Ok(shift);
        }

        //Create Shift
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ShiftCreateRequest request)
        {
            var created = await _shiftService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        //Update Shift
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ShiftUpdateRequest request)
        {
            var result = await _shiftService.UpdateAsync(request);
            return result ? Ok("Cập nhật shift thành công") : NotFound();
        }

        //Delete Shift
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _shiftService.DeleteAsync(id);
            return result ? Ok("Xóa shift thành công") : NotFound();
        }

        //Active Shift
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromQuery] bool isActive)
        {
            var result = await _shiftService.ChangeStatusAsync(id, isActive);
            return result ? Ok("Cập nhật trạng thái thành công") : NotFound();
        }

    }
}