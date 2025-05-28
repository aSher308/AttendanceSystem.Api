// Models/DTOs/DepartmentDtos.cs

using System;
using System.Collections.Generic;

namespace AttendanceSystem.Models.DTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DepartmentDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<UserDto> Users { get; set; }
    }

    public class CreateDepartmentDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UpdateDepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}