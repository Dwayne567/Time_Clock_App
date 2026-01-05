using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Models;
using Timeclock_WebApplication.Repository;

namespace Backend.Tests.Repositories
{
    public class TaskRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TaskRepository _repository;

        public TaskRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new TaskRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetAll_ReturnsAllTasks()
        {
            // Arrange
            _context.Tasks.AddRange(
                new TaskItem { Id = 1, TaskDescription = "Task A" },
                new TaskItem { Id = 2, TaskDescription = "Task B" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAll();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAll_ReturnsEmpty_WhenNoTasks()
        {
            // Act
            var result = await _repository.GetAll();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsTask_WhenExists()
        {
            // Arrange
            _context.Tasks.Add(new TaskItem { Id = 1, TaskDescription = "Test Task" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Task", result.TaskDescription);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Add_CreatesNewTask()
        {
            // Arrange
            var task = new TaskItem { TaskDescription = "New Task" };

            // Act
            var result = _repository.Add(task);

            // Assert
            Assert.True(result);
            Assert.Single(_context.Tasks);
        }

        [Fact]
        public void Delete_RemovesTask()
        {
            // Arrange
            var task = new TaskItem { Id = 1, TaskDescription = "To Delete" };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            // Act
            var result = _repository.Delete(task);

            // Assert
            Assert.True(result);
            Assert.Empty(_context.Tasks);
        }

        [Fact]
        public void Update_ModifiesTask()
        {
            // Arrange
            var task = new TaskItem { Id = 1, TaskDescription = "Original" };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            // Act
            task.TaskDescription = "Updated";
            var result = _repository.Update(task);

            // Assert
            Assert.True(result);
            Assert.Equal("Updated", _context.Tasks.First().TaskDescription);
        }

        [Fact]
        public void Save_ReturnsTrue_WhenChangesExist()
        {
            // Arrange
            _context.Tasks.Add(new TaskItem { TaskDescription = "Test" });

            // Act
            var result = _repository.Save();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Save_ReturnsFalse_WhenNoChanges()
        {
            // Act
            var result = _repository.Save();

            // Assert
            Assert.False(result);
        }
    }
}
