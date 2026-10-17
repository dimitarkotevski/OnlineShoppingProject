using FluentValidation.TestHelper;
using IntellaQuest.BusinessLogic.ViewModels;
using Xunit;

namespace IntellaQuest.Tests.Validation
{
    public class UserValidatorTests
    {
        private readonly UserRegistrationViewModelValidator _registrationValidator = new();
        private readonly UserLoginViewModelValidator _loginValidator = new();
        private readonly UserViewModelValidator _userValidator = new();
        private readonly PaymentViewModelValidator _paymentValidator = new();

        [Fact]
        public void RegistrationValidator_passes_with_valid_model()
        {
            var model = new UserRegistrationViewModel
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                FirstName = "John",
                LastName = "Doe"
            };

            var result = _registrationValidator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void RegistrationValidator_fails_when_username_missing(string username)
        {
            var model = new UserRegistrationViewModel
            {
                Email = "test@example.com",
                Username = username,
                Password = "Password123!",
                FirstName = "John",
                LastName = "Doe"
            };

            var result = _registrationValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-an-email")]
        [InlineData("plainaddress")]
        public void RegistrationValidator_fails_when_email_invalid(string email)
        {
            var model = new UserRegistrationViewModel
            {
                Email = email,
                Username = "testuser",
                Password = "Password123!",
                FirstName = "John",
                LastName = "Doe"
            };

            var result = _registrationValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void LoginValidator_passes_with_valid_model()
        {
            var model = new UserLoginViewModel
            {
                Username = "validuser",
                Password = "secretpassword"
            };

            var result = _loginValidator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("", "password")]
        [InlineData("username", "")]
        [InlineData(null, null)]
        public void LoginValidator_fails_when_credentials_missing(string username, string password)
        {
            var model = new UserLoginViewModel
            {
                Username = username,
                Password = password
            };

            var result = _loginValidator.TestValidate(model);
            if (string.IsNullOrEmpty(username))
            {
                result.ShouldHaveValidationErrorFor(x => x.Username);
            }
            if (string.IsNullOrEmpty(password))
            {
                result.ShouldHaveValidationErrorFor(x => x.Password);
            }
        }

        [Fact]
        public void UserValidator_passes_with_valid_model()
        {
            var model = new UserViewModel
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                Username = "alicesmith"
            };

            var result = _userValidator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void UserValidator_fails_when_required_fields_missing()
        {
            var model = new UserViewModel();

            var result = _userValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.FirstName);
            result.ShouldHaveValidationErrorFor(x => x.LastName);
            result.ShouldHaveValidationErrorFor(x => x.Email);
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public void PaymentValidator_fails_when_fields_are_empty()
        {
            var model = new PaymentsViewModel();

            var result = _paymentValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.PaymentType);
            result.ShouldHaveValidationErrorFor(x => x.CardNumber);
            result.ShouldHaveValidationErrorFor(x => x.SecurityCode);
        }
    }
}
