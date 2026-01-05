using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Models;
using Timeclock_WebApplication.Repository;

namespace Backend.Tests.Repositories
{
    public class DayEntryRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly DayEntryRepository _repository;

        public DayEntryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new DayEntryRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        // ========== GetAllAsync Tests ==========
        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoEntries()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllEntries()
        {
            // Arrange
            _context.DayEntries.AddRange(
                new DayEntry { Id = 1, AppUserId = "user1", Date = DateTime.Today },
                new DayEntry { Id = 2, AppUserId = "user2", Date = DateTime.Today }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        // ========== GetByIdAsync Tests ==========
        [Fact]
        public async Task GetByIdAsync_ReturnsEntry_WhenExists()
        {
            // Arrange
            var entry = new DayEntry { Id = 1, AppUserId = "user1", Date = DateTime.Today };
            _context.DayEntries.Add(entry);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user1", result.AppUserId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        // ========== GetByWeekOfAsync Tests ==========
        [Fact]
        public async Task GetByWeekOfAsync_ReturnsEntriesForWeek()
        {
            // Arrange
            var weekOf = new DateTime(2025, 1, 5); // Sunday
            _context.DayEntries.AddRange(
                new DayEntry { Id = 1, AppUserId = "user1", WeekOf = weekOf },
                new DayEntry { Id = 2, AppUserId = "user1", WeekOf = weekOf },
                new DayEntry { Id = 3, AppUserId = "user1", WeekOf = weekOf.AddDays(7) }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByWeekOfAsync(weekOf);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByWeekOfAsync_ReturnsEmpty_WhenNoEntriesForWeek()
        {
            // Arrange
            var weekOf = new DateTime(2025, 1, 5);
            _context.DayEntries.Add(new DayEntry { Id = 1, WeekOf = weekOf.AddDays(7) });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByWeekOfAsync(weekOf);

            // Assert
            Assert.Empty(result);
        }

        // ========== fetchDayEntriesByUser Tests ==========
        [Fact]
        public async Task FetchDayEntriesByUser_ReturnsUserEntriesForWeek()
        {
            // Arrange
            var weekOf = new DateTime(2025, 1, 5);
            _context.DayEntries.AddRange(
                new DayEntry { Id = 1, AppUserId = "user1", WeekOf = weekOf },
                new DayEntry { Id = 2, AppUserId = "user1", WeekOf = weekOf },
                new DayEntry { Id = 3, AppUserId = "user2", WeekOf = weekOf },
                new DayEntry { Id = 4, AppUserId = "user1", WeekOf = weekOf.AddDays(7) }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.fetchDayEntriesByUser("user1", weekOf);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, e => Assert.Equal("user1", e.AppUserId));
        }

        // ========== CreateAsync Tests ==========
        [Fact]
        public async Task CreateAsync_AddsEntryToDatabase()
        {
            // Arrange
            var entry = new DayEntry
            {
                AppUserId = "user1",
                Date = DateTime.Today,
                DayName = "Monday",
                DayStartTime = new TimeSpan(8, 0, 0)
            };

            // Act
            var result = await _repository.CreateAsync(entry);

            // Assert
            Assert.True(result.Id > 0);
            Assert.Single(_context.DayEntries);
        }

        [Fact]
        public async Task CreateAsync_ReturnsCreatedEntry()
        {
            // Arrange
            var entry = new DayEntry
            {
                AppUserId = "user1",
                Date = DateTime.Today,
                DayStartTime = new TimeSpan(9, 0, 0)
            };

            // Act
            var result = await _repository.CreateAsync(entry);

            // Assert
            Assert.Equal("user1", result.AppUserId);
            Assert.Equal(new TimeSpan(9, 0, 0), result.DayStartTime);
        }

        // ========== UpdateAsync Tests ==========
        [Fact]
        public async Task UpdateAsync_ModifiesEntry()
        {
            // Arrange
            var entry = new DayEntry
            {
                Id = 1,
                AppUserId = "user1",
                DayStartTime = new TimeSpan(8, 0, 0)
            };
            _context.DayEntries.Add(entry);
            await _context.SaveChangesAsync();
            _context.Entry(entry).State = EntityState.Detached;

            // Act
            entry.DayEndTime = new TimeSpan(17, 0, 0);
            await _repository.UpdateAsync(entry);

            // Assert
            var updated = await _context.DayEntries.FindAsync(1);
            Assert.Equal(new TimeSpan(17, 0, 0), updated!.DayEndTime);
        }

        // ========== DeleteAsync Tests ==========
        [Fact]
        public async Task DeleteAsync_RemovesEntry()
        {
            // Arrange
            var entry = new DayEntry { Id = 1, AppUserId = "user1" };
            _context.DayEntries.Add(entry);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(1);

            // Assert
            Assert.Empty(_context.DayEntries);
        }

        [Fact]
        public async Task DeleteAsync_DoesNotThrow_WhenEntryNotExists()
        {
            // Act & Assert - should not throw
            await _repository.DeleteAsync(999);
        }

        // ========== DeleteByDateAndUserIdAsync Tests ==========
        [Fact]
        public async Task DeleteByDateAndUserIdAsync_RemovesMatchingEntries()
        {
            // Arrange
            var targetDate = DateTime.Today;
            _context.DayEntries.AddRange(
                new DayEntry { Id = 1, AppUserId = "user1", Date = targetDate },
                new DayEntry { Id = 2, AppUserId = "user1", Date = targetDate },
                new DayEntry { Id = 3, AppUserId = "user1", Date = targetDate.AddDays(1) },
                new DayEntry { Id = 4, AppUserId = "user2", Date = targetDate }
            );
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteByDateAndUserIdAsync(targetDate, "user1");

            // Assert
            Assert.Equal(2, _context.DayEntries.Count());
            Assert.DoesNotContain(_context.DayEntries, e => e.AppUserId == "user1" && e.Date == targetDate);
        }
    }
}
