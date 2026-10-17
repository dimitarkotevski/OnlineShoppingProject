using System.Linq;
using NexCart.Business.Security;
using Xunit;

namespace NexCart.Tests.Security
{
    public class PasswordPolicyTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("        ")]
        public void Validate_reports_required_when_password_is_missing(string password)
        {
            var errors = PasswordPolicy.Validate(password);

            Assert.Single(errors);
            Assert.Contains("required", errors[0]);
        }

        [Fact]
        public void Validate_accepts_a_strong_password()
        {
            Assert.True(PasswordPolicy.IsValid("Str0ng!Passw0rd"));
            Assert.Empty(PasswordPolicy.Validate("Str0ng!Passw0rd"));
        }

        [Fact]
        public void Validate_rejects_a_password_that_is_too_short()
        {
            var errors = PasswordPolicy.Validate("Ab1!");

            Assert.Contains(errors, e => e.Contains("at least"));
        }

        [Fact]
        public void Validate_rejects_a_password_that_is_too_long()
        {
            string password = new string('a', PasswordPolicy.MaximumLength) + "A1!";

            var errors = PasswordPolicy.Validate(password);

            Assert.Contains(errors, e => e.Contains("at most"));
        }

        [Theory]
        [InlineData("lowercase1!", "uppercase")]
        [InlineData("UPPERCASE1!", "lowercase")]
        [InlineData("NoNumber!!", "number")]
        [InlineData("NoSpecial1", "special")]
        public void Validate_reports_each_missing_character_class(string password, string expectedFragment)
        {
            var errors = PasswordPolicy.Validate(password);

            Assert.Contains(errors, e => e.Contains(expectedFragment));
        }
    }
}
