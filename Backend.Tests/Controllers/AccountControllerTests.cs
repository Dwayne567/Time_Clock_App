using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Timeclock_WebApplication.Controllers;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Models;
using Timeclock_WebApplication.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<UserManager<AppUser>> _mockUserManager;
    private readonly Mock<SignInManager<AppUser>> _mockSignInManager;
    private readonly ApplicationDbContext _context;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        // Setup UserManager mock
        var userStore = new Mock<IUserStore<AppUser>>();
        _mockUserManager = new Mock<UserManager<AppUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // Setup SignInManager mock
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
        _mockSignInManager = new Mock<SignInManager<AppUser>>(
            _mockUserManager.Object,
            contextAccessor.Object,
            userPrincipalFactory.Object,
            null!, null!, null!, null!);

        // Setup InMemory DbContext
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _controller = new AccountController(_mockUserManager.Object, _mockSignInManager.Object, _context);
    }

    #region Login Tests

    [Fact]
    public async Task Login_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Email", "Required");
        var loginVm = new LoginViewModel();

        // Act
        var result = await _controller.Login(loginVm);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_UserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        var loginVm = new LoginViewModel
        {
            EmailAddress = "notfound@test.com",
            Password = "Password123!"
        };
        _mockUserManager.Setup(x => x.FindByEmailAsync(loginVm.EmailAddress))
            .ReturnsAsync((AppUser?)null);

        // Act
        var result = await _controller.Login(loginVm);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Wrong credentials. Please try again.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var user = new AppUser { Id = "user1", Email = "test@test.com", UserName = "test@test.com" };
        var loginVm = new LoginViewModel
        {
            EmailAddress = "test@test.com",
            Password = "WrongPassword!"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginVm.EmailAddress))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginVm.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Login(loginVm);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Wrong credentials. Please try again.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithUserInfo()
    {
        // Arrange
        var user = new AppUser { Id = "user1", Email = "test@test.com", UserName = "test@test.com" };
        var loginVm = new LoginViewModel
        {
            EmailAddress = "test@test.com",
            Password = "Password123!"
        };
        var roles = new List<string> { "User" };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginVm.EmailAddress))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginVm.Password))
            .ReturnsAsync(true);
        _mockSignInManager.Setup(x => x.PasswordSignInAsync(user, loginVm.Password, false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var result = await _controller.Login(loginVm);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Login_AdminUser_ReturnsIsAdminTrue()
    {
        // Arrange
        var user = new AppUser { Id = "admin1", Email = "admin@test.com", UserName = "admin@test.com" };
        var loginVm = new LoginViewModel
        {
            EmailAddress = "admin@test.com",
            Password = "AdminPassword123!"
        };
        var roles = new List<string> { "admin" };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginVm.EmailAddress))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginVm.Password))
            .ReturnsAsync(true);
        _mockSignInManager.Setup(x => x.PasswordSignInAsync(user, loginVm.Password, false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var result = await _controller.Login(loginVm);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Login_SignInFails_ReturnsUnauthorized()
    {
        // Arrange
        var user = new AppUser { Id = "user1", Email = "test@test.com", UserName = "test@test.com" };
        var loginVm = new LoginViewModel
        {
            EmailAddress = "test@test.com",
            Password = "Password123!"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginVm.EmailAddress))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginVm.Password))
            .ReturnsAsync(true);
        _mockSignInManager.Setup(x => x.PasswordSignInAsync(user, loginVm.Password, false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        var result = await _controller.Login(loginVm);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task Register_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Email", "Required");
        var registerVm = new RegisterViewModel();

        // Act
        var result = await _controller.Register(registerVm);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_EmailAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        var existingUser = new AppUser { Id = "user1", Email = "existing@test.com" };
        var registerVm = new RegisterViewModel
        {
            EmailAddress = "existing@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            EmployeeNumber = 1001,
            Group = "Engineering"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerVm.EmailAddress))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.Register(registerVm);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("This email address is already in use", badRequestResult.Value);
    }

    [Fact]
    public async Task Register_ValidData_ReturnsOk()
    {
        // Arrange
        var registerVm = new RegisterViewModel
        {
            EmailAddress = "newuser@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            EmployeeNumber = 1001,
            Group = "Engineering"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerVm.EmailAddress))
            .ReturnsAsync((AppUser?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), registerVm.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), UserRoles.User))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Register(registerVm);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Registration successful. You can now log in.", okResult.Value);
    }

    [Fact]
    public async Task Register_CreateUserFails_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var registerVm = new RegisterViewModel
        {
            EmailAddress = "newuser@test.com",
            Password = "weak",
            ConfirmPassword = "weak",
            FirstName = "John",
            LastName = "Doe",
            EmployeeNumber = 1001,
            Group = "Engineering"
        };

        var errors = new List<IdentityError>
        {
            new IdentityError { Code = "PasswordTooShort", Description = "Password must be at least 6 characters." }
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerVm.EmailAddress))
            .ReturnsAsync((AppUser?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), registerVm.Password))
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        // Act
        var result = await _controller.Register(registerVm);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_CreatesUserWithCorrectProperties()
    {
        // Arrange
        AppUser? createdUser = null;
        var registerVm = new RegisterViewModel
        {
            EmailAddress = "newuser@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            EmployeeNumber = 1001,
            Group = "Engineering"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerVm.EmailAddress))
            .ReturnsAsync((AppUser?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), registerVm.Password))
            .Callback<AppUser, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), UserRoles.User))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _controller.Register(registerVm);

        // Assert
        Assert.NotNull(createdUser);
        Assert.Equal(registerVm.EmailAddress, createdUser!.Email);
        Assert.Equal(registerVm.EmailAddress, createdUser.UserName);
        Assert.Equal(registerVm.FirstName, createdUser.FirstName);
        Assert.Equal(registerVm.LastName, createdUser.LastName);
        Assert.Equal(registerVm.EmployeeNumber, createdUser.EmployeeNumber);
        Assert.Equal(registerVm.Group, createdUser.Group);
        Assert.True(createdUser.EmailConfirmed);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_CallsSignOut_ReturnsOk()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.SignOutAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
    }

    #endregion
}
