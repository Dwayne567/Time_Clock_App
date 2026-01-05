using System.ComponentModel.DataAnnotations;
using Timeclock_WebApplication.ViewModels;

namespace Backend.Tests.ViewModels
{
    public class DashboardViewModelTests
    {
        [Fact]
        public void DashboardViewModel_DefaultValues()
        {
            // Arrange & Act
            var model = new DashboardViewModel();

            // Assert
            Assert.NotNull(model.DayEntries);
            Assert.NotNull(model.TaskEntries);
            Assert.NotNull(model.LeaveEntries);
            Assert.NotNull(model.Jobs);
            Assert.NotNull(model.Tasks);
        }

        [Fact]
        public void DashboardViewModel_CanSetWeekSelect()
        {
            // Arrange
            var model = new DashboardViewModel();
            var weekDate = new DateTime(2025, 1, 5);

            // Act
            model.WeekSelect = weekDate;

            // Assert
            Assert.Equal(weekDate, model.WeekSelect);
        }

        [Fact]
        public void DashboardViewModel_CanSetUserDetails()
        {
            // Arrange
            var model = new DashboardViewModel
            {
                CurrentUserId = "user-123",
                FirstName = "John",
                LastName = "Doe",
                IsAdmin = true
            };

            // Assert
            Assert.Equal("user-123", model.CurrentUserId);
            Assert.Equal("John", model.FirstName);
            Assert.Equal("Doe", model.LastName);
            Assert.True(model.IsAdmin);
        }

        [Fact]
        public void DashboardViewModel_CanSetHourTotals()
        {
            // Arrange
            var model = new DashboardViewModel
            {
                DayEntryTotalHours = 40.0,
                TaskEntryTotalHours = 35.0,
                LeaveEntryTotalHours = 8.0,
                TotalHours = 83.0
            };

            // Assert
            Assert.Equal(40.0, model.DayEntryTotalHours);
            Assert.Equal(35.0, model.TaskEntryTotalHours);
            Assert.Equal(8.0, model.LeaveEntryTotalHours);
            Assert.Equal(83.0, model.TotalHours);
        }

        [Fact]
        public void DashboardViewModel_CanSetWeekStatus()
        {
            // Arrange
            var model = new DashboardViewModel
            {
                IsPrevWeek = true,
                IsCurrentWeek = false,
                IsFutureWeek = false
            };

            // Assert
            Assert.True(model.IsPrevWeek);
            Assert.False(model.IsCurrentWeek);
            Assert.False(model.IsFutureWeek);
        }
    }
}
