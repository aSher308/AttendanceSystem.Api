using AttendanceSystem.Attributes;
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireRole("User", "Admin")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("me/summary")]
        [RequireRole("User", "Admin")]
        public async Task<IActionResult> GetSummary(DateTime from, DateTime to)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var result = await _statisticsService.GetSummaryAsync(userId.Value, from, to);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("me/leave-overtime")]
        [RequireRole("User", "Admin")]
        public async Task<IActionResult> GetLeaveAndOvertime(DateTime from, DateTime to)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var result = await _statisticsService.GetLeaveAndOvertimeAsync(userId.Value, from, to);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("export-excel")]
        [RequireRole("Admin")]
        public async Task<IActionResult> ExportToExcel(DateTime from, DateTime to)
        {
            var bytes = await _statisticsService.ExportToExcelAsync(from, to);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendanceStats.xlsx");
        }

        [HttpGet("all")]
        [RequireRole("Admin")]
        public async Task<IActionResult> GetAllStatistics(DateTime from, DateTime to)
        {
            var result = await _statisticsService.GetAllSummaryAsync(from, to);
            return Ok(result);
        }
    }
}
