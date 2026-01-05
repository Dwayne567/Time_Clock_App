using System.ComponentModel.DataAnnotations;
using Timeclock_WebApplication.ViewModels;

namespace Backend.Tests.ViewModels
{
    public class LoginViewModelTests
    {
        [Fact]
        public void LoginViewModel_RequiresEmailAddress()
        {
            // Arrange
            var model = new LoginViewModel
            {
                EmailAddress = null!,
                Password = "password123"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("EmailAddress"));
        }

        [Fact]
        public void LoginViewModel_RequiresPassword()
        {
            // Arrange
            var model = new LoginViewModel
            {
                EmailAddress = "test@example.com",
                Password = null!
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void LoginViewModel_ValidModel_PassesValidation()
        {
            // Arrange
            var model = new LoginViewModel
            {
                EmailAddress = "test@example.com",
                Password = "password123"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }

        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }
    }

    public class RegisterViewModelTests
    {
        [Fact]
        public void RegisterViewModel_RequiresEmailAddress()
        {
            // Arrange
            var model = CreateValidModel();
            model.EmailAddress = null!;

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("EmailAddress"));
        }

        [Fact]
        public void RegisterViewModel_RequiresPassword()
        {
            // Arrange
            var model = CreateValidModel();
            model.Password = null!;

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void RegisterViewModel_RequiresConfirmPassword()
        {
            // Arrange
            var model = CreateValidModel();
            model.ConfirmPassword = null!;

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("ConfirmPassword"));
        }

        [Fact]
        public void RegisterViewModel_ConfirmPassword_MustMatchPassword()
        {
            // Arrange
            var model = CreateValidModel();
            model.Password = "password123";
            model.ConfirmPassword = "differentpassword";

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("ConfirmPassword"));
        }

        [Fact]
        public void RegisterViewModel_RequiresFirstName()
        {
            // Arrange
            var model = CreateValidModel();
            model.FirstName = null;

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void RegisterViewModel_RequiresLastName()
        {
            // Arrange
            var model = CreateValidModel();
            model.LastName = null;

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("LastName"));
        }

        [Fact]
        public void RegisterViewModel_RequiresEmployeeNumber()
        {
            // Arrange
            var model = CreateValidModel();
            model.EmployeeNumber = null;

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("EmployeeNumber"));
        }

        [Fact]
        public void RegisterViewModel_RequiresGroup()
        {
            // Arrange
            var model = CreateValidModel();
            model.Group = null;

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("Group"));
        }

        [Fact]
        public void RegisterViewModel_ValidModel_PassesValidation()
        {
            // Arrange
            var model = CreateValidModel();

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }

        private static RegisterViewModel CreateValidModel()
        {
            return new RegisterViewModel
            {
                EmailAddress = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password123",
                FirstName = "John",
                LastName = "Doe",
                EmployeeNumber = 12345,
                Group = "Engineering"
            };
        }

        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }
    }
}
