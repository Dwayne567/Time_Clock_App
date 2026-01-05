using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Models;
using Timeclock_WebApplication.Repository;

namespace Backend.Tests.Repositories
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new UserRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Add Tests

        [Fact]
        public void Add_CreatesNewUser()
        {
            // Arrange
            var user = new AppUser 
            { 
                Id = "user1",
                FirstName = "John", 
                LastName = "Doe",
                Email = "john@test.com",
                UserName = "john@test.com"
            };

            // Act
            var result = _repository.Add(user);

            // Assert
            Assert.True(result);
            Assert.Single(_context.Users);
        }

        [Fact]
        public void Add_SetsUserProperties()
        {
            // Arrange
            var user = new AppUser 
            { 
                Id = "user1",
                FirstName = "John", 
                LastName = "Doe",
                Email = "john@test.com",
                UserName = "john@test.com",
                EmployeeNumber = 1001,
                Group = "Engineering"
            };

            // Act
            _repository.Add(user);

            // Assert
            var savedUser = _context.Users.Find("user1");
            Assert.NotNull(savedUser);
            Assert.Equal("John", savedUser!.FirstName);
            Assert.Equal("Doe", savedUser.LastName);
            Assert.Equal(1001, savedUser.EmployeeNumber);
            Assert.Equal("Engineering", savedUser.Group);
        }

        #endregion

        #region GetAllUsers Tests

        [Fact]
        public async Task GetAllUsers_ReturnsAllUsers()
        {
            // Arrange
            _context.Users.AddRange(
                new AppUser { Id = "user1", Email = "user1@test.com", FirstName = "John" },
                new AppUser { Id = "user2", Email = "user2@test.com", FirstName = "Jane" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllUsers();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllUsers_ReturnsEmpty_WhenNoUsers()
        {
            // Act
            var result = await _repository.GetAllUsers();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetUserById Tests

        [Fact]
        public async Task GetUserById_ReturnsUser_WhenExists()
        {
            // Arrange
            var user = new AppUser { Id = "user1", FirstName = "John", Email = "john@test.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserById("user1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
        }

        [Fact]
        public async Task GetUserById_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetUserById("nonexistent");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetUsersByGroup Tests

        [Fact]
        public async Task GetUsersByGroup_ReturnsUsersInGroup()
        {
            // Arrange
            _context.Users.AddRange(
                new AppUser { Id = "user1", Group = "Engineering", Email = "user1@test.com" },
                new AppUser { Id = "user2", Group = "Engineering", Email = "user2@test.com" },
                new AppUser { Id = "user3", Group = "Sales", Email = "user3@test.com" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUsersByGroup("Engineering");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, u => Assert.Equal("Engineering", u.Group));
        }

        [Fact]
        public async Task GetUsersByGroup_ReturnsEmpty_WhenNoUsersInGroup()
        {
            // Arrange
            _context.Users.Add(new AppUser { Id = "user1", Group = "Engineering", Email = "user1@test.com" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUsersByGroup("Marketing");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUsersByGroup_IncludesTaskEntries()
        {
            // Arrange
            var user = new AppUser { Id = "user1", Group = "Engineering", Email = "user1@test.com" };
            _context.Users.Add(user);
            _context.TaskEntries.Add(new TaskEntry { Id = 1, AppUserId = "user1", TaskName = "Dev Task" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUsersByGroup("Engineering");

            // Assert
            var resultUser = result.First();
            Assert.NotNull(resultUser.TaskEntries);
            Assert.Single(resultUser.TaskEntries);
        }

        #endregion

        #region Update Tests

        [Fact]
        public void Update_ModifiesUser()
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
            _context.SaveChanges();

            // Act
            user.FirstName = "Jane";
            user.LastName = "Smith";
            var result = _repository.Update(user);

            // Assert
            Assert.True(result);
            var updatedUser = _context.Users.Find("user1");
            Assert.Equal("Jane", updatedUser!.FirstName);
            Assert.Equal("Smith", updatedUser.LastName);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public void Delete_RemovesUser()
        {
            // Arrange
            var user = new AppUser { Id = "user1", Email = "user1@test.com" };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            var result = _repository.Delete(user);

            // Assert
            Assert.True(result);
            Assert.Empty(_context.Users);
        }

        [Fact]
        public void Delete_ReturnsFalse_WhenUserNotExists()
        {
            // Arrange
            var user = new AppUser { Id = "nonexistent", Email = "fake@test.com" };

            // Act
            var result = _repository.Delete(user);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Save Tests

        [Fact]
        public void Save_ReturnsTrue_WhenChangesExist()
        {
            // Arrange
            _context.Users.Add(new AppUser { Id = "user1", Email = "user1@test.com" });

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

        #endregion
    }
}
