using AttendanceSystem.Data;
/*using AttendanceSystem.Repositories;
using AttendanceSystem.Repositories.Interfaces;*/
using AttendanceSystem.Services;
using AttendanceSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ 1. Cấu hình DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ 2. Đăng ký repository và service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
// ✅ 3. Thêm session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ 4. Thêm CORS (nếu frontend React/Vue...)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// ✅ 5. Add controllers
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

// ✅ 6. Seed database: role + admin + shift
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedRolesAsync(dbContext);
    await DataSeeder.SeedAdminUserAsync(dbContext);
    await DataSeeder.SeedShiftsAsync(dbContext); 
}

// ✅ 7. Middleware pipeline
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseSession();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Run();
