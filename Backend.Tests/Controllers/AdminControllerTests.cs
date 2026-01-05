using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Timeclock_WebApplication.Controllers;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Interfaces;
using Timeclock_WebApplication.Models;

namespace Backend.Tests.Controllers;

public class AdminControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _mockUserRepository = new Mock<IUserRepository>();
        
        _controller = new AdminController(_context, _mockUserRepository.Object);
        SetupHttpContext("admin1", true);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SetupHttpContext(string userId, bool isAdmin)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        
        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, UserRoles.Admin));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.User).Returns(claimsPrincipal);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };
    }

    #region GetUsers Tests

    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<AppUser>
        {
            new AppUser { Id = "user1", FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new AppUser { Id = "user2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
        };
        _mockUserRepository.Setup(x => x.GetAllUsers()).ReturnsAsync(users);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUsers = Assert.IsAssignableFrom<IEnumerable<AppUser>>(okResult.Value);
        Assert.Equal(2, returnedUsers.Count());
    }

    [Fact]
    public async Task GetUsers_EmptyList_ReturnsEmptyList()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetAllUsers()).ReturnsAsync(new List<AppUser>());

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUsers = Assert.IsAssignableFrom<IEnumerable<AppUser>>(okResult.Value);
        Assert.Empty(returnedUsers);
    }

    #endregion

    #region ExportToExcel Tests

    [Fact]
    public async Task ExportToExcel_ValidGroup_ReturnsExcelFile()
    {
        // Arrange
        var users = new List<AppUser>
        {
            new AppUser 
            { 
                Id = "user1", 
                FirstName = "John", 
                LastName = "Doe", 
                EmployeeNumber = 1001,
                Email = "john@test.com",
                Group = "Engineering"
            }
        };
        
        _mockUserRepository.Setup(x => x.GetUsersByGroup("Engineering")).ReturnsAsync(users);

        // Add test data
        var job = new Job { Id = 1, JobNumber = "JOB001", JobName = "Project Alpha" };
        _context.Jobs.Add(job);
        
        var taskEntry = new TaskEntry
        {
            Id = 1,
            AppUserId = "user1",
            JobId = 1,
            Job = job,
            TaskName = "Development",
            Duration = 8.0,
            Date = DateTime.Today,
            Comment = "Working"
        };
        _context.TaskEntries.Add(taskEntry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.ExportToExcel("Engineering", DateTime.Today.AddDays(-7), DateTime.Today);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
        Assert.Equal("TaskEntries.xlsx", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task ExportToExcel_NoUsers_ReturnsEmptyExcel()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetUsersByGroup("NonExistent")).ReturnsAsync(new List<AppUser>());

        // Act
        var result = await _controller.ExportToExcel("NonExistent", DateTime.Today.AddDays(-7), DateTime.Today);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.NotNull(fileResult.FileContents);
    }

    [Fact]
    public async Task ExportToExcel_WithLeaveEntries_IncludesLeaveData()
    {
        // Arrange
        var users = new List<AppUser>
        {
            new AppUser 
            { 
                Id = "user1", 
                FirstName = "John", 
                LastName = "Doe",
                EmployeeNumber = 1001,
                Email = "john@test.com"
            }
        };
        
        _mockUserRepository.Setup(x => x.GetUsersByGroup("Engineering")).ReturnsAsync(users);

        var leaveEntry = new LeaveEntry
        {
            Id = 1,
            AppUserId = "user1",
            LeaveType = "Vacation",
            LeaveDuration = 8.0,
            Date = DateTime.Today,
            Status = "Approved"
        };
        _context.LeaveEntries.Add(leaveEntry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.ExportToExcel("Engineering", DateTime.Today.AddDays(-7), DateTime.Today);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.NotNull(fileResult.FileContents);
    }

    #endregion

    #region ExportJobDetailsToExcel Tests

    [Fact]
    public async Task ExportJobDetailsToExcel_ExistingJob_ReturnsExcelFile()
    {
        // Arrange
        var user = new AppUser 
        { 
            Id = "user1", 
            FirstName = "John", 
            LastName = "Doe",
            Email = "john@test.com"
        };
        _context.Users.Add(user);

        var job = new Job { Id = 1, JobNumber = "JOB001", JobName = "Project Alpha" };
        _context.Jobs.Add(job);

        var taskEntry = new TaskEntry
        {
            Id = 1,
            AppUserId = "user1",
            JobId = 1,
            TaskName = "Development",
            Duration = 8.0,
            Date = DateTime.Today
        };
        _context.TaskEntries.Add(taskEntry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.ExportJobDetailsToExcel("JOB001");

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
    }

    [Fact]
    public async Task ExportJobDetailsToExcel_NonExistingJob_ReturnsEmptyExcel()
    {
        // Act
        var result = await _controller.ExportJobDetailsToExcel("NONEXISTENT");

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.NotNull(fileResult.FileContents);
    }

    #endregion
}
