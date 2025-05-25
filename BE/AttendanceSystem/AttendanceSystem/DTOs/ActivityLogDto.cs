// Models/DTOs/ActivityLogDto.cs
using System;

namespace AttendanceSystem.Models.DTOs
{
    public class ActivityLogDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string IPAddress { get; set; }
        public string DeviceInfo { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}