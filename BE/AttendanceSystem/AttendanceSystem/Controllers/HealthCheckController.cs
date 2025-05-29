using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Data;

[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly AppDbContext _context;

    public HealthCheckController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Check()
    {
        try
        {
            var canConnectDb = await _context.Database.CanConnectAsync();

            if (!canConnectDb)
            {
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Database = "Disconnected"
                });
            }

            return Ok(new
            {
                Status = "Healthy",
                Time = DateTime.UtcNow,
                Database = "Connected"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Status = "Unhealthy",
                Error = ex.Message
            });
        }
    }
}
