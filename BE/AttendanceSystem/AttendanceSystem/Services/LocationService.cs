using AttendanceSystem.Data;
using AttendanceSystem.DTOs;
using AttendanceSystem.Models;
using AttendanceSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Services
{
    public class LocationService : ILocationService
    {
        private readonly AppDbContext _context;

        public LocationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Location>> GetAllAsync()
        {
            return await _context.Locations.ToListAsync();
        }

        public async Task<Location?> GetByIdAsync(int id)
        {
            return await _context.Locations.FindAsync(id);
        }

        public async Task<Location> CreateAsync(LocationCreateRequest request)
        {
            if (request.IsDefault)
            {
                var currentDefault = await _context.Locations.FirstOrDefaultAsync(l => l.IsDefault);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                }
            }

            var location = new Location
            {
                Name = request.Name,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                RadiusInMeters = request.RadiusInMeters,
                IsDefault = request.IsDefault
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<bool> UpdateAsync(LocationUpdateRequest request)
        {
            var location = await _context.Locations.FindAsync(request.Id);
            if (location == null) return false;

            if (request.IsDefault && !location.IsDefault)
            {
                var currentDefault = await _context.Locations.FirstOrDefaultAsync(l => l.IsDefault);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                }
            }

            location.Name = request.Name;
            location.Latitude = request.Latitude;
            location.Longitude = request.Longitude;
            location.RadiusInMeters = request.RadiusInMeters;
            location.IsDefault = request.IsDefault;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) return false;

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
