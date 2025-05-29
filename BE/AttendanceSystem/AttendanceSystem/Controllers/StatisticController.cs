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
        public async Task<ActionResult<StatisticDTO>> GetStatistics(
            [FromQuery] int? userId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return Unauthorized();

            var isAdmin = await _accountService.IsAdminAsync(currentUserId.Value);
            if (!isAdmin)
            {
                userId = currentUserId.Value; // Ép userId về chính mình
            }

            try
            {
                var statistics = await _statisticService.GetStatisticsAsync(userId, fromDate, toDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportStatistics(
            [FromQuery] int? userId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return Unauthorized();

            var isAdmin = await _accountService.IsAdminAsync(currentUserId.Value);
            if (!isAdmin)
            {
                userId = currentUserId.Value; // Ép userId về chính mình
            }

            try
            {
                var excelBytes = await _statisticService.ExportStatisticsToExcelAsync(userId, fromDate, toDate);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"ThongKe_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}