using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Models;
using Timeclock_WebApplication.Repository;

namespace Backend.Tests.Repositories
{
    public class JobRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly JobRepository _repository;

        public JobRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new JobRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetAll_ReturnsAllJobs()
        {
            // Arrange
            _context.Jobs.AddRange(
                new Job { Id = 1, JobNumber = "J001", JobName = "Project A" },
                new Job { Id = 2, JobNumber = "J002", JobName = "Project B" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAll();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAll_ReturnsEmpty_WhenNoJobs()
        {
            // Act
            var result = await _repository.GetAll();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task CreateAsync_AddsJob()
        {
            // Arrange
            var job = new Job { JobNumber = "J001", JobName = "New Job" };

            // Act
            var result = await _repository.CreateAsync(job);

            // Assert
            Assert.True(result.Id > 0);
            Assert.Single(_context.Jobs);
        }

        [Fact]
        public async Task CreateAsync_ReturnsCreatedJob()
        {
            // Arrange
            var job = new Job { JobNumber = "J001", JobName = "Test Job" };

            // Act
            var result = await _repository.CreateAsync(job);

            // Assert
            Assert.Equal("J001", result.JobNumber);
            Assert.Equal("Test Job", result.JobName);
        }

        [Fact]
        public void Add_CreatesNewJob()
        {
            // Arrange
            var job = new Job { JobNumber = "J001", JobName = "New Job" };

            // Act
            var result = _repository.Add(job);

            // Assert
            Assert.True(result);
            Assert.Single(_context.Jobs);
        }

        [Fact]
        public void Delete_RemovesJob()
        {
            // Arrange
            var job = new Job { Id = 1, JobNumber = "J001", JobName = "To Delete" };
            _context.Jobs.Add(job);
            _context.SaveChanges();

            // Act
            var result = _repository.Delete(job);

            // Assert
            Assert.True(result);
            Assert.Empty(_context.Jobs);
        }

        [Fact]
        public void Update_ModifiesJob()
        {
            // Arrange
            var job = new Job { Id = 1, JobNumber = "J001", JobName = "Original" };
            _context.Jobs.Add(job);
            _context.SaveChanges();

            // Act
            job.JobName = "Updated";
            var result = _repository.Update(job);

            // Assert
            Assert.True(result);
            Assert.Equal("Updated", _context.Jobs.First().JobName);
        }

        [Fact]
        public async Task FindByJobNumberAsync_ReturnsJob_WhenExists()
        {
            // Arrange
            _context.Jobs.Add(new Job { Id = 1, JobNumber = "ABC123", JobName = "Found Job" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindByJobNumberAsync("ABC123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Found Job", result.JobName);
        }

        [Fact]
        public async Task FindByJobNumberAsync_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.FindByJobNumberAsync("NOTFOUND");

            // Assert
            Assert.Null(result);
        }
    }
}

