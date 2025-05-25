// Controllers/ActivityLogsController.cs
using AttendanceSystem.Attributes;
using AttendanceSystem.Models.DTOs;
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AttendanceSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireRole("Admin")]
    public class ActivityLogsController : ControllerBase
    {
        private readonly IActivityLogService _logService;

        public ActivityLogsController(IActivityLogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ActivityLogDto>>> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? userId = null)
        {
            var result = await _logService.GetPagedLogsAsync(pageNumber, pageSize, fromDate, toDate, userId);
            return Ok(result);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var excelBytes = await _logService.ExportExcelAsync(fromDate, toDate);
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ActivityLogs_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}