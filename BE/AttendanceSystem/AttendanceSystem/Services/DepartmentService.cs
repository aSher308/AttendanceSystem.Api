// Services/DepartmentService.cs

using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Models.DTOs;
using AttendanceSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceSystem.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly AppDbContext _context;

        public DepartmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = await _context.Departments
                .AsNoTracking()
                .ToListAsync();

            return departments.Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name
            });
        }

        public async Task<DepartmentDetailDto> GetDepartmentByIdAsync(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Users)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null) return null;

            return new DepartmentDetailDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt,
                Users = department.Users?.Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email
                }).ToList()
            };
        }

        public async Task<DepartmentDetailDto> CreateDepartmentAsync(CreateDepartmentDto departmentDto)
        {
            var department = new Department
            {
                Name = departmentDto.Name,
                Description = departmentDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return new DepartmentDetailDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt,
                Users = new List<UserDto>()
            };
        }

        public async Task<DepartmentDetailDto> UpdateDepartmentAsync(int id, UpdateDepartmentDto departmentDto)
        {
            var existingDepartment = await _context.Departments.FindAsync(id);
            if (existingDepartment == null) return null;

            existingDepartment.Name = departmentDto.Name;
            existingDepartment.Description = departmentDto.Description;
            existingDepartment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new DepartmentDetailDto
            {
                Id = existingDepartment.Id,
                Name = existingDepartment.Name,
                Description = existingDepartment.Description,
                CreatedAt = existingDepartment.CreatedAt,
                UpdatedAt = existingDepartment.UpdatedAt
            };
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return false;

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}