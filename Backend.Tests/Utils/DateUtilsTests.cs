using Timeclock_WebApplication.Utils;

namespace Backend.Tests.Utils
{
    public class DateUtilsTests
    {
        [Fact]
        public void StartOfWeek_Sunday_ReturnsCorrectDate()
        {
            // Arrange - Wednesday, January 8, 2025
            var wednesday = new DateTime(2025, 1, 8);

            // Act
            var result = wednesday.StartOfWeek(DayOfWeek.Sunday);

            // Assert - Should return Sunday, January 5, 2025
            Assert.Equal(new DateTime(2025, 1, 5), result);
        }

        [Fact]
        public void StartOfWeek_Monday_ReturnsCorrectDate()
        {
            // Arrange - Wednesday, January 8, 2025
            var wednesday = new DateTime(2025, 1, 8);

            // Act
            var result = wednesday.StartOfWeek(DayOfWeek.Monday);

            // Assert - Should return Monday, January 6, 2025
            Assert.Equal(new DateTime(2025, 1, 6), result);
        }

        [Fact]
        public void StartOfWeek_WhenDateIsStartOfWeek_ReturnsSameDate()
        {
            // Arrange - Sunday, January 5, 2025
            var sunday = new DateTime(2025, 1, 5);

            // Act
            var result = sunday.StartOfWeek(DayOfWeek.Sunday);

            // Assert
            Assert.Equal(sunday, result);
        }

        [Fact]
        public void StartOfWeek_Saturday_ReturnsCorrectDate()
        {
            // Arrange - Wednesday, January 8, 2025
            var wednesday = new DateTime(2025, 1, 8);

            // Act
            var result = wednesday.StartOfWeek(DayOfWeek.Saturday);

            // Assert - Should return Saturday, January 4, 2025
            Assert.Equal(new DateTime(2025, 1, 4), result);
        }

        [Fact]
        public void StartOfWeek_ReturnsDateWithoutTime()
        {
            // Arrange - Wednesday with time component
            var dateWithTime = new DateTime(2025, 1, 8, 14, 30, 45);

            // Act
            var result = dateWithTime.StartOfWeek(DayOfWeek.Sunday);

            // Assert - Should return date with no time component
            Assert.Equal(TimeSpan.Zero, result.TimeOfDay);
        }

        [Fact]
        public void StartOfWeek_EndOfYear_ReturnsCorrectDate()
        {
            // Arrange - Wednesday, December 31, 2025
            var lastDayOfYear = new DateTime(2025, 12, 31);

            // Act
            var result = lastDayOfYear.StartOfWeek(DayOfWeek.Sunday);

            // Assert - Should return Sunday, December 28, 2025
            Assert.Equal(new DateTime(2025, 12, 28), result);
        }

        [Fact]
        public void StartOfWeek_CrossingYearBoundary_ReturnsCorrectDate()
        {
            // Arrange - Thursday, January 2, 2025
            var earlyJan = new DateTime(2025, 1, 2);

            // Act
            var result = earlyJan.StartOfWeek(DayOfWeek.Sunday);

            // Assert - Should return Sunday, December 29, 2024
            Assert.Equal(new DateTime(2024, 12, 29), result);
        }
    }
}
