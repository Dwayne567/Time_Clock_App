using FT_TTMS_WebApplication.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FT_TTMS_WebApplication.Data
{
    // Application database context class
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet properties for various entities
        public DbSet<DayEntry>? DayEntries { get; set; }
        public DbSet<LeaveEntry>? LeaveEntries { get; set; }
        public DbSet<TimeEntry>? TimeEntries { get; set; }
        public DbSet<Job>? Jobs { get; set; }
        public DbSet<ImportedJob>? ImportedJobs { get; set; }
        public DbSet<CreatedJob> CreatedJobs { get; set; }
        public DbSet<Duty>? Duties { get; set; }
        public DbSet<Passwords> Passwords { get; set; }

        // Configures the model properties
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure properties for DayEntry entity
            modelBuilder.Entity<DayEntry>(entity =>
            {
                entity.Property(e => e.WeekOf).HasColumnType("date");
                entity.Property(e => e.Date).HasColumnType("date");
                entity.Property(e => e.DayStartTime).HasColumnType("time(0)");
                entity.Property(e => e.DayEndTime).HasColumnType("time(0)");
                entity.Property(e => e.LunchStartTime).HasColumnType("time(0)");
                entity.Property(e => e.LunchEndTime).HasColumnType("time(0)");
            });

            // Configure properties for LeaveEntry entity
            modelBuilder.Entity<LeaveEntry>(entity =>
            {
                entity.Property(e => e.WeekOf).HasColumnType("date");
                entity.Property(e => e.Date).HasColumnType("date");
            });

            // Configure properties for TimeEntry entity
            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.Property(e => e.WeekOf).HasColumnType("date");
                entity.Property(e => e.Date).HasColumnType("date");
            });
        }
    }
}