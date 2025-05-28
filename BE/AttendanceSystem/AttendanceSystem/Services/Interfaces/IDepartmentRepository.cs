// Services/Interfaces/IDepartmentService.cs

using AttendanceSystem.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttendanceSystem.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDetailDto> GetDepartmentByIdAsync(int id);
        Task<DepartmentDetailDto> CreateDepartmentAsync(CreateDepartmentDto departmentDto);
        Task<DepartmentDetailDto> UpdateDepartmentAsync(int id, UpdateDepartmentDto departmentDto);
        Task<bool> DeleteDepartmentAsync(int id);
    }
}