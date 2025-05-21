using AttendanceSystem.Data;
using AttendanceSystem.Services;
using AttendanceSystem.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Converters;


var builder = WebApplication.CreateBuilder(args);

// Đăng ký repository và service
builder.Services.AddScoped<IUserService, UserService>();

// ✅ 1. Cấu hình DbContext (kết nối SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ 2. Đăng ký Service

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IWorkScheduleService, WorkScheduleService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<IEmailService, EmailService>();


// Thêm session

// ✅ 3. Cấu hình Session dùng cookie

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // ⚠️ RẤT QUAN TRỌNG KHI DÙNG CORS + COOKIE
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});


// Thêm CORS (nếu frontend React/Vue...)

// ✅ 4. CORS cho frontend (React)

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "https://localhost:5173") // ✅ React URL
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials();
              }); // ✅ bắt buộc để dùng cookie

});


// Hangfire config
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// Add controllers

// ✅ 5. Add controller + Swagger

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new TimeSpanJsonConverter());
    });

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

// ✅ 6. Seed dữ liệu mặc định (admin, roles)

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedRolesAsync(dbContext);
    await DataSeeder.SeedAdminUserAsync(dbContext);
    await DataSeeder.SeedShiftsAsync(dbContext); 
}

 
// Middleware pipeline
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseSession();
app.UseAuthorization();
app.MapControllers();

// Enable Hangfire Dashboard
app.UseHangfireDashboard();

// Đăng ký job tự động đánh vắng mỗi ngày lúc 23h
AttendanceService.ScheduleDailyAbsentJob();

 
// ✅ 7. Middleware pipeline thứ tự CHUẨN
 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

// ⚠️ Đặt trước session & authorization
app.UseCors("AllowFrontend");
app.UseSession();
app.UseAuthorization();
app.MapControllers();
app.Run();
