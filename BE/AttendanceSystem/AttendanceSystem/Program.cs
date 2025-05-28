using AttendanceSystem.Data;
using AttendanceSystem.Middleware;
using AttendanceSystem.Services;
using AttendanceSystem.Services.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ---------------------- CẤU HÌNH DỊCH VỤ ----------------------

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories & Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IWorkScheduleService, WorkScheduleService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".AttendanceSystem.Session";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// CORS (chỉ cho phép FE từ localhost:5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Authentication - Cookie-based
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".AttendanceSystem.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None; // Quan trọng cho cross-origin cookie
        options.LoginPath = "/Account/Login"; // Điều chỉnh theo controller của bạn
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// Swagger
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

//builder.Services.AddControllers(); //Lưu ý
//test
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();



// ---------------------- SEED DATABASE ----------------------
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedRolesAsync(dbContext);
    await DataSeeder.SeedAdminUserAsync(dbContext);
    await DataSeeder.SeedShiftsAsync(dbContext);
    await DataSeeder.SeedLocationsAsync(dbContext);
    await DataSeeder.SeedDepartmentsAsync(dbContext);
}

// ---------------------- MIDDLEWARE PIPELINE ----------------------
app.UseErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseHangfireDashboard();

app.MapControllers();

// ---------------------- JOB TỰ ĐỘNG ----------------------
AttendanceService.ScheduleDailyAbsentJob();

app.Run();
