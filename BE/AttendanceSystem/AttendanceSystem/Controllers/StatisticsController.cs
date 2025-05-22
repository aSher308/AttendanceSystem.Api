using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("user/{userId}/summary")]
        public async Task<IActionResult> GetSummary(int userId, DateTime from, DateTime to)
        {
            var result = await _statisticsService.GetSummaryAsync(userId, from, to);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("user/{userId}/leave-overtime")]
        public async Task<IActionResult> GetLeaveAndOvertime(int userId, DateTime from, DateTime to)
        {
            var result = await _statisticsService.GetLeaveAndOvertimeAsync(userId, from, to);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportToExcel(DateTime from, DateTime to)
        {
            var bytes = await _statisticsService.ExportToExcelAsync(from, to);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendanceStats.xlsx");
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllStatistics(DateTime from, DateTime to)
        {
            var result = await _statisticsService.GetAllSummaryAsync(from, to);
            return Ok(result);
        }
    }
}
