using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Models;
using Timeclock_WebApplication.Repository;

namespace Backend.Tests.Repositories
{
    public class LeaveEntryRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly LeaveEntryRepository _repository;

        public LeaveEntryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new LeaveEntryRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ReturnsAllEntries()
        {
            // Arrange
            _context.LeaveEntries.AddRange(
                new LeaveEntry { Id = 1, LeaveType = "Vacation", AppUserId = "user1" },
                new LeaveEntry { Id = 2, LeaveType = "Sick", AppUserId = "user2" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoEntries()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByWeekOfAsync Tests

        [Fact]
        public async Task GetByWeekOfAsync_ReturnsEntriesForWeek()
        {
            // Arrange
            var weekOf = new DateTime(2025, 1, 5);
            _context.LeaveEntries.AddRange(
                new LeaveEntry { Id = 1, WeekOf = weekOf, LeaveType = "Vacation" },
                new LeaveEntry { Id = 2, WeekOf = weekOf, LeaveType = "Sick" },
                new LeaveEntry { Id = 3, WeekOf = weekOf.AddDays(7), LeaveType = "PTO" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByWeekOfAsync(weekOf);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByWeekOfAsync_ReturnsEmpty_WhenNoMatchingWeek()
        {
            // Arrange
            _context.LeaveEntries.Add(new LeaveEntry { Id = 1, WeekOf = new DateTime(2025, 1, 5) });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByWeekOfAsync(new DateTime(2025, 2, 5));

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region FetchLeaveEntriesByUser Tests

        [Fact]
        public async Task FetchLeaveEntriesByUser_ReturnsUserEntriesForWeek()
        {
            // Arrange
            var weekOf = new DateTime(2025, 1, 5);
            _context.LeaveEntries.AddRange(
                new LeaveEntry { Id = 1, AppUserId = "user1", WeekOf = weekOf, LeaveType = "Vacation" },
                new LeaveEntry { Id = 2, AppUserId = "user1", WeekOf = weekOf, LeaveType = "Sick" },
                new LeaveEntry { Id = 3, AppUserId = "user2", WeekOf = weekOf, LeaveType = "PTO" },
                new LeaveEntry { Id = 4, AppUserId = "user1", WeekOf = weekOf.AddDays(7), LeaveType = "Holiday" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FetchLeaveEntriesByUser("user1", weekOf);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, e => Assert.Equal("user1", e.AppUserId));
        }

        [Fact]
        public async Task FetchLeaveEntriesByUser_ReturnsEmpty_WhenNoMatchingUser()
        {
            // Arrange
            var weekOf = new DateTime(2025, 1, 5);
            _context.LeaveEntries.Add(new LeaveEntry { Id = 1, AppUserId = "user1", WeekOf = weekOf });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FetchLeaveEntriesByUser("user999", weekOf);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsEntry_WhenExists()
        {
            // Arrange
            var entry = new LeaveEntry { Id = 1, LeaveType = "Vacation", LeaveDuration = 8.0 };
            _context.LeaveEntries.Add(entry);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Vacation", result.LeaveType);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_AddsEntry()
        {
            // Arrange
            var entry = new LeaveEntry 
            { 
                LeaveType = "Vacation", 
                LeaveDuration = 8.0,
                AppUserId = "user1",
                Date = DateTime.Today
            };

            // Act
            var result = await _repository.CreateAsync(entry);

            // Assert
            Assert.True(result.Id > 0);
            Assert.Single(_context.LeaveEntries);
        }

        [Fact]
        public async Task CreateAsync_ReturnsCreatedEntry()
        {
            // Arrange
            var entry = new LeaveEntry 
            { 
                LeaveType = "Sick",
                LeaveDuration = 4.0 
            };

            // Act
            var result = await _repository.CreateAsync(entry);

            // Assert
            Assert.Equal("Sick", result.LeaveType);
            Assert.Equal(4.0, result.LeaveDuration);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ModifiesEntry()
        {
            // Arrange
            var entry = new LeaveEntry { Id = 1, LeaveType = "Vacation", LeaveDuration = 8.0 };
            _context.LeaveEntries.Add(entry);
            await _context.SaveChangesAsync();
            _context.Entry(entry).State = EntityState.Detached;

            // Act
            var updatedEntry = new LeaveEntry { Id = 1, LeaveType = "PTO", LeaveDuration = 4.0 };
            await _repository.UpdateAsync(updatedEntry);

            // Assert
            var result = await _context.LeaveEntries.FindAsync(1);
            Assert.Equal("PTO", result!.LeaveType);
            Assert.Equal(4.0, result.LeaveDuration);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_RemovesEntry()
        {
            // Arrange
            var entry = new LeaveEntry { Id = 1, LeaveType = "Vacation" };
            _context.LeaveEntries.Add(entry);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(1);

            // Assert
            Assert.Empty(_context.LeaveEntries);
        }

        [Fact]
        public async Task DeleteAsync_DoesNothing_WhenEntryNotExists()
        {
            // Arrange
            _context.LeaveEntries.Add(new LeaveEntry { Id = 1, LeaveType = "Vacation" });
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(999);

            // Assert
            Assert.Single(_context.LeaveEntries);
        }

        #endregion
    }
}
