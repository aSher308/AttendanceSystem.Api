using AttendanceSystem.Attributes;
using AttendanceSystem.DTOs;
using AttendanceSystem.Services.Interfaces;
using Hangfire.Storage.Monitoring;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticService _statisticService;
        private readonly IAccountService _accountService;

        public StatisticController(
            IStatisticService statisticService,
            IAccountService accountService)
        {
            _statisticService = statisticService;
            _accountService = accountService;
        }

        [HttpGet]
        [RequireRole("User", "Admin")]
        public async Task<ActionResult<StatisticDTO>> GetStatistics(
        [FromQuery] int? userId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return Unauthorized();

            var isAdmin = await _accountService.IsAdminAsync(currentUserId.Value);
            if (!isAdmin)
            {
                userId = currentUserId.Value;
            }

            // Nếu không truyền fromDate và toDate thì lấy tháng hiện tại
            var now = DateTime.Now;
            var start = fromDate ?? new DateTime(now.Year, now.Month, 1);
            var end = toDate ?? start.AddMonths(1).AddDays(-1); // đến cuối tháng

            try
            {
                var statistics = await _statisticService.GetStatisticsAsync(userId, start, end);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("export")]
        [RequireRole("User", "Admin")]
        public async Task<IActionResult> ExportStatistics(
            [FromQuery] int? userId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return Unauthorized();

            var isAdmin = await _accountService.IsAdminAsync(currentUserId.Value);
            if (!isAdmin)
            {
                userId = currentUserId.Value;
            }

            var now = DateTime.Now;
            var start = fromDate ?? new DateTime(now.Year, now.Month, 1);
            var end = toDate ?? start.AddMonths(1).AddDays(-1);

            try
            {
                var excelBytes = await _statisticService.ExportStatisticsToExcelAsync(userId, start, end);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"ThongKe_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}