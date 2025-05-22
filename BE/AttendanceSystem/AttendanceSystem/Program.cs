using AttendanceSystem.Data;
using AttendanceSystem.Middleware;
using AttendanceSystem.Services;
using AttendanceSystem.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký repository và service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IWorkScheduleService, WorkScheduleService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
/*builder.Services.AddScoped<IDepartmentService, DepartmentService>();*/
// Thêm session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Thêm CORS (nếu frontend React/Vue...)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Hangfire config
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Attendance System API",
        Version = "v1",
        Description = "API hệ thống chấm công online có đăng nhập, phân quyền, xác thực email"
    });
});
var app = builder.Build();

// Seed database: role + admin + shift
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedRolesAsync(dbContext);
    await DataSeeder.SeedAdminUserAsync(dbContext);
    await DataSeeder.SeedShiftsAsync(dbContext);
    await DataSeeder.SeedLocationsAsync(dbContext);
}

app.UseErrorHandling();
// Middleware pipeline
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseSession();
app.UseAuthorization();
app.MapControllers();

// Enable Hangfire Dashboard
app.UseHangfireDashboard();

// Đăng ký job tự động đánh vắng mỗi ngày lúc 23h
AttendanceService.ScheduleDailyAbsentJob();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Run();
