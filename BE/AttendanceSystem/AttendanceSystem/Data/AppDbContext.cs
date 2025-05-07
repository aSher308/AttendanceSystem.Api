using AttendanceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<Shift> Shifts { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }

        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<OvertimeRequest> OvertimeRequests { get; set; }

        public DbSet<QRCodeEntry> QRCodeEntries { get; set; }
        public DbSet<Location> Locations { get; set; }

        public DbSet<SystemNotification> SystemNotifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        public DbSet<AttendanceHistory> AttendanceHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite key for UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Enum as string
            modelBuilder.Entity<Attendance>()
                .Property(a => a.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Attendance>()
                .Property(a => a.AttendanceType)
                .HasConversion<string>();

            modelBuilder.Entity<LeaveRequest>()
                .Property(l => l.Status)
                .HasConversion<string>();

            modelBuilder.Entity<OvertimeRequest>()
                .Property(o => o.Status)
                .HasConversion<string>();

            modelBuilder.Entity<QRCodeEntry>()
                .Property(q => q.ScanType)
                .HasConversion<string>();

            modelBuilder.Entity<SystemNotification>()
                .Property(n => n.NotificationType)
                .HasConversion<string>();

            // Quan hệ giữa Attendance và AttendanceHistory
            modelBuilder.Entity<AttendanceHistory>()
                .HasOne(a => a.Attendance)
                .WithMany()
                .HasForeignKey(a => a.AttendanceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ giữa AttendanceHistory và User (ChangedByUser)
            modelBuilder.Entity<AttendanceHistory>()
                .HasOne(a => a.ChangedByUser)
                .WithMany()
                .HasForeignKey(a => a.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
