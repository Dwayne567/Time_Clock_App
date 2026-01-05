using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Models;
using Timeclock_WebApplication.Repository;

namespace Backend.Tests.Repositories
{
    public class TaskEntryRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TaskEntryRepository _repository;

        public TaskEntryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new TaskEntryRepository(_context);
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
            _context.TaskEntries.AddRange(
                new TaskEntry { Id = 1, TaskName = "Development", AppUserId = "user1" },
                new TaskEntry { Id = 2, TaskName = "Testing", AppUserId = "user2" }
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
            _context.TaskEntries.AddRange(
                new TaskEntry { Id = 1, WeekOf = weekOf, TaskName = "Task 1" },
                new TaskEntry { Id = 2, WeekOf = weekOf, TaskName = "Task 2" },
                new TaskEntry { Id = 3, WeekOf = weekOf.AddDays(7), TaskName = "Task 3" }
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
            _context.TaskEntries.Add(new TaskEntry { Id = 1, WeekOf = new DateTime(2025, 1, 5) });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByWeekOfAsync(new DateTime(2025, 2, 5));

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region FetchTaskEntriesByUser Tests

        [Fact]
        public async Task FetchTaskEntriesByUser_ReturnsUserEntriesForWeek()
        {
            // Arrange
            var weekOf = new DateTime(2025, 1, 5);
            _context.TaskEntries.AddRange(
                new TaskEntry { Id = 1, AppUserId = "user1", WeekOf = weekOf, TaskName = "Task 1" },
                new TaskEntry { Id = 2, AppUserId = "user1", WeekOf = weekOf, TaskName = "Task 2" },
                new TaskEntry { Id = 3, AppUserId = "user2", WeekOf = weekOf, TaskName = "Task 3" },
                new TaskEntry { Id = 4, AppUserId = "user1", WeekOf = weekOf.AddDays(7), TaskName = "Task 4" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FetchTaskEntriesByUser("user1", weekOf);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, e => Assert.Equal("user1", e.AppUserId));
        }

        [Fact]
        public async Task FetchTaskEntriesByUser_ReturnsEmpty_WhenNoMatchingUser()
        {
            // Arrange
            var weekOf = new DateTime(2025, 1, 5);
            _context.TaskEntries.Add(new TaskEntry { Id = 1, AppUserId = "user1", WeekOf = weekOf });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FetchTaskEntriesByUser("user999", weekOf);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetLastTaskEntryByUserAsync Tests

        [Fact]
        public async Task GetLastTaskEntryByUserAsync_ReturnsLatestEntry()
        {
            // Arrange
            _context.TaskEntries.AddRange(
                new TaskEntry { Id = 1, AppUserId = "user1", Date = new DateTime(2025, 1, 1), TaskName = "Old Task" },
                new TaskEntry { Id = 2, AppUserId = "user1", Date = new DateTime(2025, 1, 5), TaskName = "Latest Task" },
                new TaskEntry { Id = 3, AppUserId = "user1", Date = new DateTime(2025, 1, 3), TaskName = "Middle Task" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLastTaskEntryByUserAsync("user1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Latest Task", result.TaskName);
        }

        [Fact]
        public async Task GetLastTaskEntryByUserAsync_ReturnsNull_WhenNoEntries()
        {
            // Act
            var result = await _repository.GetLastTaskEntryByUserAsync("user1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetLastTaskEntryByUserAsync_OnlyReturnsForSpecifiedUser()
        {
            // Arrange
            _context.TaskEntries.AddRange(
                new TaskEntry { Id = 1, AppUserId = "user1", Date = new DateTime(2025, 1, 1), TaskName = "User1 Task" },
                new TaskEntry { Id = 2, AppUserId = "user2", Date = new DateTime(2025, 1, 5), TaskName = "User2 Task" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLastTaskEntryByUserAsync("user1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("User1 Task", result.TaskName);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsEntry_WhenExists()
        {
            // Arrange
            var entry = new TaskEntry { Id = 1, TaskName = "Development", Duration = 8.0 };
            _context.TaskEntries.Add(entry);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Development", result.TaskName);
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
            var entry = new TaskEntry 
            { 
                TaskName = "Development", 
                Duration = 8.0,
                AppUserId = "user1",
                Date = DateTime.Today
            };

            // Act
            var result = await _repository.CreateAsync(entry);

            // Assert
            Assert.True(result.Id > 0);
            Assert.Single(_context.TaskEntries);
        }

        [Fact]
        public async Task CreateAsync_ReturnsCreatedEntry()
        {
            // Arrange
            var entry = new TaskEntry 
            { 
                TaskName = "Testing",
                Duration = 4.0 
            };

            // Act
            var result = await _repository.CreateAsync(entry);

            // Assert
            Assert.Equal("Testing", result.TaskName);
            Assert.Equal(4.0, result.Duration);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ModifiesEntry()
        {
            // Arrange
            var entry = new TaskEntry { Id = 1, TaskName = "Development", Duration = 8.0 };
            _context.TaskEntries.Add(entry);
            await _context.SaveChangesAsync();
            _context.Entry(entry).State = EntityState.Detached;

            // Act
            var updatedEntry = new TaskEntry { Id = 1, TaskName = "Updated Task", Duration = 4.0 };
            await _repository.UpdateAsync(updatedEntry);

            // Assert
            var result = await _context.TaskEntries.FindAsync(1);
            Assert.Equal("Updated Task", result!.TaskName);
            Assert.Equal(4.0, result.Duration);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_RemovesEntry()
        {
            // Arrange
            var entry = new TaskEntry { Id = 1, TaskName = "Development" };
            _context.TaskEntries.Add(entry);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(1);

            // Assert
            Assert.Empty(_context.TaskEntries);
        }

        [Fact]
        public async Task DeleteAsync_DoesNothing_WhenEntryNotExists()
        {
            // Arrange
            _context.TaskEntries.Add(new TaskEntry { Id = 1, TaskName = "Development" });
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(999);

            // Assert
            Assert.Single(_context.TaskEntries);
        }

        #endregion

        #region DeleteByDateAndUserIdAsync Tests

        [Fact]
        public async Task DeleteByDateAndUserIdAsync_RemovesMatchingEntries()
        {
            // Arrange
            var date = new DateTime(2025, 1, 5);
            _context.TaskEntries.AddRange(
                new TaskEntry { Id = 1, AppUserId = "user1", Date = date, TaskName = "Task 1" },
                new TaskEntry { Id = 2, AppUserId = "user1", Date = date, TaskName = "Task 2" },
                new TaskEntry { Id = 3, AppUserId = "user2", Date = date, TaskName = "Task 3" },
                new TaskEntry { Id = 4, AppUserId = "user1", Date = date.AddDays(1), TaskName = "Task 4" }
            );
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteByDateAndUserIdAsync(date, "user1");

            // Assert
            Assert.Equal(2, _context.TaskEntries.Count());
            Assert.DoesNotContain(_context.TaskEntries, e => e.AppUserId == "user1" && e.Date == date);
        }

        [Fact]
        public async Task DeleteByDateAndUserIdAsync_DoesNothing_WhenNoMatch()
        {
            // Arrange
            var date = new DateTime(2025, 1, 5);
            _context.TaskEntries.Add(new TaskEntry { Id = 1, AppUserId = "user1", Date = date });
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteByDateAndUserIdAsync(date, "user999");

            // Assert
            Assert.Single(_context.TaskEntries);
        }

        #endregion

        #region FetchByGroupAndDateRangeAsync Tests

        [Fact]
        public async Task FetchByGroupAndDateRangeAsync_ReturnsEntriesForGroup()
        {
            // Arrange
            var user1 = new AppUser { Id = "user1", Group = "Engineering", Email = "user1@test.com" };
            var user2 = new AppUser { Id = "user2", Group = "Sales", Email = "user2@test.com" };
            _context.Users.AddRange(user1, user2);

            _context.TaskEntries.AddRange(
                new TaskEntry { Id = 1, AppUserId = "user1", Date = new DateTime(2025, 1, 5) },
                new TaskEntry { Id = 2, AppUserId = "user2", Date = new DateTime(2025, 1, 5) }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FetchByGroupAndDateRangeAsync("Engineering", null, null);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task FetchByGroupAndDateRangeAsync_FiltersDateRange()
        {
            // Arrange
            var user = new AppUser { Id = "user1", Group = "Engineering", Email = "user1@test.com" };
            _context.Users.Add(user);

            _context.TaskEntries.AddRange(
                new TaskEntry { Id = 1, AppUserId = "user1", Date = new DateTime(2025, 1, 1) },
                new TaskEntry { Id = 2, AppUserId = "user1", Date = new DateTime(2025, 1, 5) },
                new TaskEntry { Id = 3, AppUserId = "user1", Date = new DateTime(2025, 1, 10) }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FetchByGroupAndDateRangeAsync(
                "Engineering", 
                new DateTime(2025, 1, 3), 
                new DateTime(2025, 1, 7)
            );

            // Assert
            Assert.Single(result);
            Assert.Equal(new DateTime(2025, 1, 5), result.First().Date);
        }

        #endregion
    }
}
