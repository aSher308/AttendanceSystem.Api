namespace AttendanceSystem.DTOs
{
    public class UserResponse //Thông tin User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public int LeaveBalance { get; set; }
        public string? AvatarUrl { get; set; }
        public string? DepartmentName { get; set; }
    }
}
