using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Timeclock_WebApplication.Controllers;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Interfaces;
using Timeclock_WebApplication.Models;
using Timeclock_WebApplication.ViewModels;

namespace Backend.Tests.Controllers;

public class DashboardControllerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IDayEntryRepository> _mockDayEntryRepository;
    private readonly Mock<ILeaveEntryRepository> _mockLeaveEntryRepository;
    private readonly Mock<ITaskEntryRepository> _mockTaskEntryRepository;
    private readonly Mock<IJobRepository> _mockJobRepository;
    private readonly Mock<ITaskRepository> _mockTaskRepository;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockDayEntryRepository = new Mock<IDayEntryRepository>();
        _mockLeaveEntryRepository = new Mock<ILeaveEntryRepository>();
        _mockTaskEntryRepository = new Mock<ITaskEntryRepository>();
        _mockJobRepository = new Mock<IJobRepository>();
        _mockTaskRepository = new Mock<ITaskRepository>();

        _controller = new DashboardController(
            _mockUserRepository.Object,
            _mockDayEntryRepository.Object,
            _mockLeaveEntryRepository.Object,
            _mockTaskEntryRepository.Object,
            _mockJobRepository.Object,
            _mockTaskRepository.Object
        );

        // Setup default HttpContext with authenticated user
        SetupHttpContext("user1", false);
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

    #region Index Tests

    [Fact]
    public async Task Index_ValidRequest_ReturnsOkWithDashboardModel()
    {
        // Arrange
        var user = new AppUser { Id = "user1", FirstName = "John", LastName = "Doe" };
        _mockUserRepository.Setup(x => x.GetUserById("user1")).ReturnsAsync(user);
        _mockJobRepository.Setup(x => x.GetAll()).ReturnsAsync(new List<Job>());
        _mockTaskRepository.Setup(x => x.GetAll()).ReturnsAsync(new List<TaskItem>());
        _mockDayEntryRepository.Setup(x => x.fetchDayEntriesByUser(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<DayEntry>());
        _mockTaskEntryRepository.Setup(x => x.FetchTaskEntriesByUser(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TaskEntry>());
        _mockLeaveEntryRepository.Setup(x => x.FetchLeaveEntriesByUser(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<LeaveEntry>());
        _mockTaskEntryRepository.Setup(x => x.GetLastTaskEntryByUserAsync(It.IsAny<string>()))
            .ReturnsAsync((TaskEntry?)null);

        // Act
        var result = await _controller.Index();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var model = Assert.IsType<DashboardViewModel>(okResult.Value);
        Assert.Equal("John", model.FirstName);
        Assert.Equal("Doe", model.LastName);
    }

    [Fact]
    public async Task Index_AsAdmin_IncludesGroups()
    {
        // Arrange
        SetupHttpContext("admin1", true);
        var users = new List<AppUser>
        {
            new AppUser { Id = "user1", Group = "Engineering" },
            new AppUser { Id = "user2", Group = "Sales" },
            new AppUser { Id = "user3", Group = "Engineering" }
        };
        
        _mockUserRepository.Setup(x => x.GetUserById("admin1")).ReturnsAsync(new AppUser { Id = "admin1" });
        _mockUserRepository.Setup(x => x.GetAllUsers()).ReturnsAsync(users);
        _mockJobRepository.Setup(x => x.GetAll()).ReturnsAsync(new List<Job>());
        _mockTaskRepository.Setup(x => x.GetAll()).ReturnsAsync(new List<TaskItem>());
        _mockDayEntryRepository.Setup(x => x.fetchDayEntriesByUser(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<DayEntry>());
        _mockTaskEntryRepository.Setup(x => x.FetchTaskEntriesByUser(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TaskEntry>());
        _mockLeaveEntryRepository.Setup(x => x.FetchLeaveEntriesByUser(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<LeaveEntry>());
        _mockTaskEntryRepository.Setup(x => x.GetLastTaskEntryByUserAsync(It.IsAny<string>()))
            .ReturnsAsync((TaskEntry?)null);

        // Act
        var result = await _controller.Index();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var model = Assert.IsType<DashboardViewModel>(okResult.Value);
        Assert.True(model.IsAdmin);
        Assert.Contains("Engineering", model.Groups);
        Assert.Contains("Sales", model.Groups);
    }

    #endregion

    #region ClockInOut Tests

    [Fact]
    public async Task ClockInOut_NullDayEntry_ReturnsBadRequest()
    {
        // Arrange
        var model = new DashboardViewModel { DayEntry = null };

        // Act
        var result = await _controller.ClockInOut(model);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("DayEntry is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task ClockInOut_NewEntry_CreatesEntry()
    {
        // Arrange
        var dayEntry = new DayEntry
        {
            Id = 0,
            AppUserId = "user1",
            Date = DateTime.Today,
            DayStartTime = TimeSpan.FromHours(8)
        };
        var model = new DashboardViewModel { DayEntry = dayEntry };

        _mockDayEntryRepository.Setup(x => x.CreateAsync(It.IsAny<DayEntry>()))
            .ReturnsAsync(dayEntry);

        // Act
        var result = await _controller.ClockInOut(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockDayEntryRepository.Verify(x => x.CreateAsync(It.IsAny<DayEntry>()), Times.Once);
    }

    [Fact]
    public async Task ClockInOut_ExistingEntry_UpdatesEntry()
    {
        // Arrange
        var existingEntry = new DayEntry
        {
            Id = 1,
            AppUserId = "user1",
            Date = DateTime.Today,
            DayStartTime = TimeSpan.FromHours(8)
        };
        var dayEntry = new DayEntry
        {
            Id = 1,
            AppUserId = "user1",
            Date = DateTime.Today,
            DayStartTime = TimeSpan.FromHours(8),
            DayEndTime = TimeSpan.FromHours(17)
        };
        var model = new DashboardViewModel { DayEntry = dayEntry };

        _mockDayEntryRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingEntry);
        _mockDayEntryRepository.Setup(x => x.UpdateAsync(It.IsAny<DayEntry>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ClockInOut(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockDayEntryRepository.Verify(x => x.UpdateAsync(It.IsAny<DayEntry>()), Times.Once);
    }

    [Fact]
    public async Task ClockInOut_CalculatesDurations_Correctly()
    {
        // Arrange
        DayEntry? createdEntry = null;
        var dayEntry = new DayEntry
        {
            Id = 0,
            AppUserId = "user1",
            Date = DateTime.Today,
            DayStartTime = TimeSpan.FromHours(8),
            DayEndTime = TimeSpan.FromHours(17),
            LunchStartTime = TimeSpan.FromHours(12),
            LunchEndTime = TimeSpan.FromHours(13)
        };
        var model = new DashboardViewModel { DayEntry = dayEntry };

        _mockDayEntryRepository.Setup(x => x.CreateAsync(It.IsAny<DayEntry>()))
            .Callback<DayEntry>(e => createdEntry = e)
            .ReturnsAsync(dayEntry);

        // Act
        await _controller.ClockInOut(model);

        // Assert
        Assert.NotNull(createdEntry);
        Assert.Equal(9.0, createdEntry!.DayDuration); // 17 - 8 = 9 hours
        Assert.Equal(1.0, createdEntry.LunchDuration); // 13 - 12 = 1 hour
        Assert.Equal(8.0, createdEntry.WorkDuration); // 9 - 1 = 8 hours
    }

    #endregion

    #region AddTaskEntry Tests

    [Fact]
    public async Task AddTaskEntry_NullTaskEntry_ReturnsBadRequest()
    {
        // Arrange
        var model = new DashboardViewModel { TaskEntry = null };

        // Act
        var result = await _controller.AddTaskEntry(model);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("TaskEntry is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddTaskEntry_NewEntry_CreatesEntry()
    {
        // Arrange
        var taskEntry = new TaskEntry { Id = 0, TaskName = "Development", Duration = 4.0 };
        var model = new DashboardViewModel { TaskEntry = taskEntry };

        _mockTaskEntryRepository.Setup(x => x.CreateAsync(It.IsAny<TaskEntry>()))
            .ReturnsAsync(taskEntry);

        // Act
        var result = await _controller.AddTaskEntry(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Task entry added/updated successfully", okResult.Value);
        _mockTaskEntryRepository.Verify(x => x.CreateAsync(It.IsAny<TaskEntry>()), Times.Once);
    }

    [Fact]
    public async Task AddTaskEntry_ExistingEntry_UpdatesEntry()
    {
        // Arrange
        var taskEntry = new TaskEntry { Id = 1, TaskName = "Development", Duration = 4.0 };
        var model = new DashboardViewModel { TaskEntry = taskEntry };

        _mockTaskEntryRepository.Setup(x => x.UpdateAsync(It.IsAny<TaskEntry>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddTaskEntry(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockTaskEntryRepository.Verify(x => x.UpdateAsync(It.IsAny<TaskEntry>()), Times.Once);
    }

    #endregion

    #region DeleteTaskEntry Tests

    [Fact]
    public async Task DeleteTaskEntry_ExistingEntry_DeletesAndReturnsOk()
    {
        // Arrange
        var taskEntry = new TaskEntry { Id = 1, TaskName = "Development" };
        _mockTaskEntryRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(taskEntry);
        _mockTaskEntryRepository.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteTaskEntry(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Task entry deleted successfully.", okResult.Value);
    }

    [Fact]
    public async Task DeleteTaskEntry_NonExistingEntry_ReturnsNotFound()
    {
        // Arrange
        _mockTaskEntryRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((TaskEntry?)null);

        // Act
        var result = await _controller.DeleteTaskEntry(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Task entry not found.", notFoundResult.Value);
    }

    #endregion

    #region DeleteDay Tests

    [Fact]
    public async Task DeleteDay_ExistingEntry_DeletesAndReturnsOk()
    {
        // Arrange
        var dayEntry = new DayEntry { Id = 1, Date = DateTime.Today };
        _mockDayEntryRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(dayEntry);
        _mockDayEntryRepository.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteDay(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Day entry deleted successfully.", okResult.Value);
    }

    [Fact]
    public async Task DeleteDay_NonExistingEntry_ReturnsNotFound()
    {
        // Arrange
        _mockDayEntryRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((DayEntry?)null!);

        // Act
        var result = await _controller.DeleteDay(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Day entry not found.", notFoundResult.Value);
    }

    #endregion

    #region AddJob Tests

    [Fact]
    public async Task AddJob_NullJobModel_ReturnsBadRequest()
    {
        // Arrange
        var model = new DashboardViewModel { JobModel = null };

        // Act
        var result = await _controller.AddJob(model);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Job name and number are required.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddJob_EmptyJobNumber_ReturnsBadRequest()
    {
        // Arrange
        var model = new DashboardViewModel { JobModel = new Job { JobNumber = "" } };

        // Act
        var result = await _controller.AddJob(model);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddJob_DuplicateJobNumber_ReturnsBadRequest()
    {
        // Arrange
        var existingJob = new Job { Id = 1, JobNumber = "JOB001", JobName = "Existing" };
        var model = new DashboardViewModel { JobModel = new Job { JobNumber = "JOB001", JobName = "New" } };
        
        _mockJobRepository.Setup(x => x.FindByJobNumberAsync("JOB001")).ReturnsAsync(existingJob);

        // Act
        var result = await _controller.AddJob(model);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("already exists", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task AddJob_ValidJob_CreatesAndReturnsOk()
    {
        // Arrange
        var model = new DashboardViewModel 
        { 
            JobModel = new Job { JobNumber = "JOB001", JobName = "New Project" } 
        };
        
        _mockJobRepository.Setup(x => x.FindByJobNumberAsync("JOB001")).ReturnsAsync((Job?)null!);
        _mockJobRepository.Setup(x => x.CreateAsync(It.IsAny<Job>())).ReturnsAsync(new Job());

        // Act
        var result = await _controller.AddJob(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Job created successfully.", okResult.Value);
    }

    #endregion

    #region AddLeave Tests

    [Fact]
    public async Task AddLeave_NullLeaveEntry_ReturnsBadRequest()
    {
        // Arrange
        var model = new DashboardViewModel { LeaveEntry = null };

        // Act
        var result = await _controller.AddLeave(model);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("LeaveEntry is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddLeave_NewEntry_CreatesEntry()
    {
        // Arrange
        var leaveEntry = new LeaveEntry { Id = 0, LeaveType = "Vacation", LeaveDuration = 8.0 };
        var model = new DashboardViewModel { LeaveEntry = leaveEntry };

        _mockLeaveEntryRepository.Setup(x => x.CreateAsync(It.IsAny<LeaveEntry>()))
            .ReturnsAsync(leaveEntry);

        // Act
        var result = await _controller.AddLeave(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Leave entry added/updated successfully", okResult.Value);
    }

    [Fact]
    public async Task AddLeave_ExistingEntry_UpdatesEntry()
    {
        // Arrange
        var leaveEntry = new LeaveEntry { Id = 1, LeaveType = "Vacation", LeaveDuration = 8.0 };
        var model = new DashboardViewModel { LeaveEntry = leaveEntry };

        _mockLeaveEntryRepository.Setup(x => x.UpdateAsync(It.IsAny<LeaveEntry>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddLeave(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockLeaveEntryRepository.Verify(x => x.UpdateAsync(It.IsAny<LeaveEntry>()), Times.Once);
    }

    #endregion

    #region DeleteLeave Tests

    [Fact]
    public async Task DeleteLeave_ExistingEntry_DeletesAndReturnsOk()
    {
        // Arrange
        var leaveEntry = new LeaveEntry { Id = 1, LeaveType = "Vacation" };
        _mockLeaveEntryRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(leaveEntry);
        _mockLeaveEntryRepository.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteLeave(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Leave entry deleted successfully.", okResult.Value);
    }

    [Fact]
    public async Task DeleteLeave_NonExistingEntry_ReturnsNotFound()
    {
        // Arrange
        _mockLeaveEntryRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((LeaveEntry?)null);

        // Act
        var result = await _controller.DeleteLeave(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Leave entry not found.", notFoundResult.Value);
    }

    #endregion

    #region ExportTimeSheet Tests

    [Fact]
    public async Task ExportTimeSheet_EmptyGroup_ReturnsBadRequest()
    {
        // Arrange
        SetupHttpContext("admin1", true);

        // Act
        var result = await _controller.ExportTimeSheet(group: "", fromDate: null, toDate: null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Group is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task ExportTimeSheet_ValidGroup_ReturnsCsvFile()
    {
        // Arrange
        SetupHttpContext("admin1", true);
        var taskEntries = new List<TaskEntry>
        {
            new TaskEntry 
            { 
                Id = 1, 
                Date = DateTime.Today, 
                TaskName = "Development",
                Duration = 8.0,
                Comment = "Working on feature",
                AppUser = new AppUser { FirstName = "John", LastName = "Doe", Email = "john@test.com", Group = "Engineering" },
                Job = new Job { JobNumberAndJobName = "JOB001 - Project Alpha" }
            }
        };
        
        _mockTaskEntryRepository.Setup(x => x.FetchByGroupAndDateRangeAsync("Engineering", It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(taskEntries);

        // Act
        var result = await _controller.ExportTimeSheet(group: "Engineering", fromDate: null, toDate: null);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", fileResult.ContentType);
        Assert.StartsWith("timesheet_Engineering_", fileResult.FileDownloadName);
    }

    #endregion
}
