using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Controllers;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Models;

namespace Backend.Tests.Controllers;

public class JobsControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly JobsController _controller;

    public JobsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _controller = new JobsController(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetJobs Tests

    [Fact]
    public async Task GetJobs_NoJobs_ReturnsEmptyList()
    {
        // Act
        var result = await _controller.GetJobs();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetJobs_WithJobs_ReturnsAllJobs()
    {
        // Arrange
        _context.Jobs.AddRange(
            new Job { Id = 1, JobNumber = "JOB001", JobName = "Project Alpha" },
            new Job { Id = 2, JobNumber = "JOB002", JobName = "Project Beta" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetJobs();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetJobs_WithSearchTerm_FiltersResults()
    {
        // Arrange
        _context.Jobs.AddRange(
            new Job { Id = 1, JobNumber = "JOB001", JobName = "Project Alpha" },
            new Job { Id = 2, JobNumber = "JOB002", JobName = "Project Beta" },
            new Job { Id = 3, JobNumber = "TASK003", JobName = "Task Gamma" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetJobs(searchTerm: "JOB");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetJobs_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 25; i++)
        {
            _context.Jobs.Add(new Job { Id = i, JobNumber = $"JOB{i:D3}", JobName = $"Project {i}" });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetJobs(pageNumber: 2, pageSize: 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    #endregion

    #region GetJob Tests

    [Fact]
    public async Task GetJob_ExistingId_ReturnsJob()
    {
        // Arrange
        var job = new Job { Id = 1, JobNumber = "JOB001", JobName = "Project Alpha" };
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetJob(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedJob = Assert.IsType<Job>(okResult.Value);
        Assert.Equal("JOB001", returnedJob.JobNumber);
    }

    [Fact]
    public async Task GetJob_NonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetJob(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region UpdateJob Tests

    [Fact]
    public async Task UpdateJob_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var job = new Job { Id = 2, JobNumber = "JOB002", JobName = "Updated" };

        // Act
        var result = await _controller.UpdateJob(1, job);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateJob_ValidJob_ReturnsNoContent()
    {
        // Arrange
        var job = new Job { Id = 1, JobNumber = "JOB001", JobName = "Project Alpha" };
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
        _context.Entry(job).State = EntityState.Detached;

        var updatedJob = new Job { Id = 1, JobNumber = "JOB001", JobName = "Updated Project" };

        // Act
        var result = await _controller.UpdateJob(1, updatedJob);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateJob_NonExistingJob_ReturnsNotFound()
    {
        // Arrange
        var job = new Job { Id = 999, JobNumber = "JOB999", JobName = "Nonexistent" };

        // Act
        var result = await _controller.UpdateJob(999, job);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region DeleteJob Tests

    [Fact]
    public async Task DeleteJob_ExistingJob_ReturnsOk()
    {
        // Arrange
        var job = new Job { Id = 1, JobNumber = "JOB001", JobName = "Project Alpha" };
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteJob(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Job deleted successfully.", okResult.Value);
        Assert.Null(await _context.Jobs.FindAsync(1));
    }

    [Fact]
    public async Task DeleteJob_NonExistingJob_ReturnsNotFound()
    {
        // Act
        var result = await _controller.DeleteJob(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteJob_JobWithTaskEntries_ReturnsBadRequest()
    {
        // Arrange
        var job = new Job { Id = 1, JobNumber = "JOB001", JobName = "Project Alpha" };
        _context.Jobs.Add(job);
        
        var taskEntry = new TaskEntry 
        { 
            Id = 1, 
            JobId = 1, 
            AppUserId = "user1", 
            TaskName = "Test Task",
            Date = DateTime.Now 
        };
        _context.TaskEntries.Add(taskEntry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteJob(1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("associated task entries", badRequestResult.Value?.ToString());
    }

    #endregion

    #region JobDetails Tests

    [Fact]
    public async Task JobDetails_ExistingJob_ReturnsTaskEntries()
    {
        // Arrange
        var user = new AppUser 
        { 
            Id = "user1", 
            FirstName = "John", 
            LastName = "Doe",
            UserName = "john@test.com",
            Email = "john@test.com"
        };
        _context.Users.Add(user);

        var job = new Job { Id = 1, JobNumber = "JOB001", JobName = "Project Alpha" };
        _context.Jobs.Add(job);

        var taskEntry = new TaskEntry 
        { 
            Id = 1, 
            JobId = 1, 
            AppUserId = "user1",
            TaskName = "Development",
            Duration = 8.0,
            Date = DateTime.Today
        };
        _context.TaskEntries.Add(taskEntry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.JobDetails("JOB001");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task JobDetails_NoMatchingJob_ReturnsEmptyList()
    {
        // Act
        var result = await _controller.JobDetails("NONEXISTENT");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    #endregion
}
