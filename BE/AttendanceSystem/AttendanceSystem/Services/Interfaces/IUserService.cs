using AttendanceSystem.DTOs;
using AttendanceSystem.Models;

namespace AttendanceSystem.Services.Interfaces
{
    public interface IUserService
    {
        // 
        Task<List<UserResponse>> GetFilteredAsync(int currentUserId, bool isAdmin, string? keyword, int? departmentId, bool? isActive, string? role);

        // 
        Task<UserResponse> CreateAsync(UserCreateRequest request);

        // 
        Task<bool> UpdateAsync(UserUpdateRequest request);

        // 
        Task<bool> ChangeStatusAsync(int userId, bool isActive);

        // 
        Task<bool> AssignRoleAsync(int userId, int roleId);

        // 
        Task<bool> ForceResetPasswordAsync(int userId, string newPassword);

        // 
        Task<UserResponse?> GetByIdAsync(int id);

        // 
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

        // 
        Task<bool> UpdateAvatarAsync(int userId, IFormFile file);

        // 
        Task<bool> IsAdminAsync(int userId);

        // 
        Task<int> ImportUsersFromExcelAsync(IFormFile file);

        //
        Task<List<string>> GetRolesOfUser(int userId);
        Task<bool> DeleteAsync(int id);
        Task<bool> RemoveRoleAsync(int userId, int roleId);

    }
}
