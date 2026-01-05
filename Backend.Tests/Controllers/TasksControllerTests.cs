using Microsoft.AspNetCore.Mvc;
using Moq;
using Timeclock_WebApplication.Controllers;
using Timeclock_WebApplication.Interfaces;
using Timeclock_WebApplication.Models;

namespace Backend.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskRepository> _mockTaskRepository;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _mockTaskRepository = new Mock<ITaskRepository>();
        _controller = new TasksController(_mockTaskRepository.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, TaskDescription = "Development" },
            new TaskItem { Id = 2, TaskDescription = "Testing" }
        };
        _mockTaskRepository.Setup(x => x.GetAll()).ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value);
        Assert.Equal(2, returnedTasks.Count());
    }

    [Fact]
    public async Task GetAll_EmptyList_ReturnsEmptyList()
    {
        // Arrange
        _mockTaskRepository.Setup(x => x.GetAll()).ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value);
        Assert.Empty(returnedTasks);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingId_ReturnsTask()
    {
        // Arrange
        var task = new TaskItem { Id = 1, TaskDescription = "Development" };
        _mockTaskRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(task);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTask = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal("Development", returnedTask.TaskDescription);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        _mockTaskRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Create Tests

    [Fact]
    public void Create_NullTask_ReturnsBadRequest()
    {
        // Act
        var result = _controller.Create(null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Create_EmptyDescription_ReturnsBadRequest()
    {
        // Arrange
        var task = new TaskItem { TaskDescription = "" };

        // Act
        var result = _controller.Create(task);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Task description is required.", badRequestResult.Value);
    }

    [Fact]
    public void Create_WhitespaceDescription_ReturnsBadRequest()
    {
        // Arrange
        var task = new TaskItem { TaskDescription = "   " };

        // Act
        var result = _controller.Create(task);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Create_ValidTask_ReturnsCreatedAtAction()
    {
        // Arrange
        var task = new TaskItem { TaskDescription = "New Task" };
        _mockTaskRepository.Setup(x => x.Add(It.IsAny<TaskItem>())).Returns(true);

        // Act
        var result = _controller.Create(task);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(TasksController.GetById), createdResult.ActionName);
    }

    [Fact]
    public void Create_RepositoryFails_ReturnsServerError()
    {
        // Arrange
        var task = new TaskItem { TaskDescription = "New Task" };
        _mockTaskRepository.Setup(x => x.Add(It.IsAny<TaskItem>())).Returns(false);

        // Act
        var result = _controller.Create(task);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public void Create_TrimsDescription()
    {
        // Arrange
        TaskItem? addedTask = null;
        var task = new TaskItem { TaskDescription = "  Task with spaces  " };
        _mockTaskRepository.Setup(x => x.Add(It.IsAny<TaskItem>()))
            .Callback<TaskItem>(t => addedTask = t)
            .Returns(true);

        // Act
        _controller.Create(task);

        // Assert
        Assert.NotNull(addedTask);
        Assert.Equal("Task with spaces", addedTask!.TaskDescription);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_NullTask_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Update(1, null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_EmptyDescription_ReturnsBadRequest()
    {
        // Arrange
        var task = new TaskItem { Id = 1, TaskDescription = "" };

        // Act
        var result = await _controller.Update(1, task);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_TaskNotFound_ReturnsNotFound()
    {
        // Arrange
        var task = new TaskItem { Id = 999, TaskDescription = "Updated" };
        _mockTaskRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _controller.Update(999, task);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ValidTask_ReturnsOk()
    {
        // Arrange
        var existingTask = new TaskItem { Id = 1, TaskDescription = "Original" };
        var updatedTask = new TaskItem { Id = 1, TaskDescription = "Updated" };
        
        _mockTaskRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingTask);
        _mockTaskRepository.Setup(x => x.Update(It.IsAny<TaskItem>())).Returns(true);

        // Act
        var result = await _controller.Update(1, updatedTask);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTask = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal("Updated", returnedTask.TaskDescription);
    }

    [Fact]
    public async Task Update_RepositoryFails_ReturnsServerError()
    {
        // Arrange
        var existingTask = new TaskItem { Id = 1, TaskDescription = "Original" };
        var updatedTask = new TaskItem { Id = 1, TaskDescription = "Updated" };
        
        _mockTaskRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingTask);
        _mockTaskRepository.Setup(x => x.Update(It.IsAny<TaskItem>())).Returns(false);

        // Act
        var result = await _controller.Update(1, updatedTask);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_TaskNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockTaskRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ValidTask_ReturnsNoContent()
    {
        // Arrange
        var task = new TaskItem { Id = 1, TaskDescription = "To Delete" };
        _mockTaskRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(task);
        _mockTaskRepository.Setup(x => x.Delete(task)).Returns(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_RepositoryFails_ReturnsServerError()
    {
        // Arrange
        var task = new TaskItem { Id = 1, TaskDescription = "To Delete" };
        _mockTaskRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(task);
        _mockTaskRepository.Setup(x => x.Delete(task)).Returns(false);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    #endregion
}
