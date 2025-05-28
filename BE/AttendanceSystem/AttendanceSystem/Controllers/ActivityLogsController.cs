using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogsController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;

        public ActivityLogsController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? userId)
        {
            var logs = await _activityLogService.GetLogsAsync(from, to, userId);
            return Ok(logs);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? userId)
        {
            var content = await _activityLogService.ExportLogsToExcelAsync(from, to, userId);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ActivityLogs.xlsx");
        }
    }

}
