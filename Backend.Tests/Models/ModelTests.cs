using Timeclock_WebApplication.Models;

namespace Backend.Tests.Models
{
    public class DayEntryTests
    {
        [Fact]
        public void DayEntry_DefaultValues_AreNull()
        {
            // Arrange & Act
            var entry = new DayEntry();

            // Assert
            Assert.Equal(0, entry.Id);
            Assert.Null(entry.AppUserId);
            Assert.Null(entry.WeekOf);
            Assert.Null(entry.Date);
            Assert.Null(entry.DayName);
            Assert.Null(entry.DayStartTime);
            Assert.Null(entry.DayEndTime);
        }

        [Fact]
        public void DayEntry_CanSetAllProperties()
        {
            // Arrange
            var entry = new DayEntry
            {
                Id = 1,
                AppUserId = "user-123",
                WeekOf = new DateTime(2025, 1, 5),
                Date = new DateTime(2025, 1, 6),
                DayName = "Monday",
                DayStartTime = new TimeSpan(8, 0, 0),
                DayEndTime = new TimeSpan(17, 0, 0),
                LunchStartTime = new TimeSpan(12, 0, 0),
                LunchEndTime = new TimeSpan(13, 0, 0),
                DayDuration = 9.0,
                LunchDuration = 1.0,
                WorkDuration = 8.0,
                Comment = "Worked on project",
                Status = "Complete"
            };

            // Assert
            Assert.Equal(1, entry.Id);
            Assert.Equal("user-123", entry.AppUserId);
            Assert.Equal(new DateTime(2025, 1, 5), entry.WeekOf);
            Assert.Equal(new DateTime(2025, 1, 6), entry.Date);
            Assert.Equal("Monday", entry.DayName);
            Assert.Equal(new TimeSpan(8, 0, 0), entry.DayStartTime);
            Assert.Equal(new TimeSpan(17, 0, 0), entry.DayEndTime);
            Assert.Equal(new TimeSpan(12, 0, 0), entry.LunchStartTime);
            Assert.Equal(new TimeSpan(13, 0, 0), entry.LunchEndTime);
            Assert.Equal(9.0, entry.DayDuration);
            Assert.Equal(1.0, entry.LunchDuration);
            Assert.Equal(8.0, entry.WorkDuration);
            Assert.Equal("Worked on project", entry.Comment);
            Assert.Equal("Complete", entry.Status);
        }

        [Fact]
        public void DayEntry_NavigationProperty_CanBeSet()
        {
            // Arrange
            var user = new AppUser { Id = "user-123", FirstName = "John", LastName = "Doe" };
            var entry = new DayEntry
            {
                AppUserId = user.Id,
                AppUser = user
            };

            // Assert
            Assert.NotNull(entry.AppUser);
            Assert.Equal("John", entry.AppUser.FirstName);
        }
    }

    public class JobTests
    {
        [Fact]
        public void Job_DefaultValues()
        {
            // Arrange & Act
            var job = new Job();

            // Assert
            Assert.Equal(0, job.Id);
            Assert.Null(job.JobNumber);
            Assert.Null(job.JobName);
        }

        [Fact]
        public void Job_CanSetProperties()
        {
            // Arrange
            var job = new Job
            {
                Id = 1,
                JobNumber = "J001",
                JobName = "Test Project"
            };

            // Assert
            Assert.Equal(1, job.Id);
            Assert.Equal("J001", job.JobNumber);
            Assert.Equal("Test Project", job.JobName);
        }
    }

    public class TaskItemTests
    {
        [Fact]
        public void TaskItem_DefaultValues()
        {
            // Arrange & Act
            var task = new TaskItem();

            // Assert
            Assert.Equal(0, task.Id);
            Assert.Null(task.TaskDescription);
        }

        [Fact]
        public void TaskItem_CanSetProperties()
        {
            // Arrange
            var task = new TaskItem
            {
                Id = 1,
                TaskDescription = "Development"
            };

            // Assert
            Assert.Equal(1, task.Id);
            Assert.Equal("Development", task.TaskDescription);
        }
    }

    public class LeaveEntryTests
    {
        [Fact]
        public void LeaveEntry_DefaultValues()
        {
            // Arrange & Act
            var entry = new LeaveEntry();

            // Assert
            Assert.Equal(0, entry.Id);
            Assert.Null(entry.AppUserId);
            Assert.Null(entry.LeaveType);
        }

        [Fact]
        public void LeaveEntry_CanSetProperties()
        {
            // Arrange
            var entry = new LeaveEntry
            {
                Id = 1,
                AppUserId = "user-123",
                WeekOf = new DateTime(2025, 1, 5),
                Date = new DateTime(2025, 1, 7),
                DayName = "Tuesday",
                LeaveType = "PTO",
                LeaveDuration = 8.0
            };

            // Assert
            Assert.Equal(1, entry.Id);
            Assert.Equal("user-123", entry.AppUserId);
            Assert.Equal(new DateTime(2025, 1, 5), entry.WeekOf);
            Assert.Equal(new DateTime(2025, 1, 7), entry.Date);
            Assert.Equal("Tuesday", entry.DayName);
            Assert.Equal("PTO", entry.LeaveType);
            Assert.Equal(8.0, entry.LeaveDuration);
        }
    }

    public class TaskEntryTests
    {
        [Fact]
        public void TaskEntry_DefaultValues()
        {
            // Arrange & Act
            var entry = new TaskEntry();

            // Assert
            Assert.Equal(0, entry.Id);
            Assert.Null(entry.AppUserId);
            Assert.Null(entry.JobId);
        }

        [Fact]
        public void TaskEntry_CanSetProperties()
        {
            // Arrange
            var entry = new TaskEntry
            {
                Id = 1,
                AppUserId = "user-123",
                JobId = 10,
                TaskName = "Development",
                WeekOf = new DateTime(2025, 1, 5),
                Date = new DateTime(2025, 1, 6),
                DayName = "Monday",
                Duration = 4.5,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(13, 30, 0),
                Comment = "Worked on feature",
                Status = "Complete"
            };

            // Assert
            Assert.Equal(1, entry.Id);
            Assert.Equal("user-123", entry.AppUserId);
            Assert.Equal(10, entry.JobId);
            Assert.Equal("Development", entry.TaskName);
            Assert.Equal(new DateTime(2025, 1, 5), entry.WeekOf);
            Assert.Equal(new DateTime(2025, 1, 6), entry.Date);
            Assert.Equal("Monday", entry.DayName);
            Assert.Equal(4.5, entry.Duration);
            Assert.Equal(new TimeSpan(9, 0, 0), entry.StartTime);
            Assert.Equal(new TimeSpan(13, 30, 0), entry.EndTime);
            Assert.Equal("Worked on feature", entry.Comment);
            Assert.Equal("Complete", entry.Status);
        }

        [Fact]
        public void TaskEntry_NavigationProperties_CanBeSet()
        {
            // Arrange
            var job = new Job { Id = 1, JobNumber = "J001", JobName = "Project" };

            var entry = new TaskEntry
            {
                JobId = job.Id,
                Job = job,
                TaskName = "Dev"
            };

            // Assert
            Assert.NotNull(entry.Job);
            Assert.Equal("J001", entry.Job.JobNumber);
            Assert.Equal("Dev", entry.TaskName);
        }
    }
}
