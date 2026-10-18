using System;
using System.Linq;
using System.Linq.Expressions;
using NexCart.Business.Exceptions;
using NexCart.Business.Exceptions.ExceptionMassages;
using NexCart.Business.Security;
using NexCart.Business.Services;
using NexCart.Business.ViewModels;
using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Domain;
using NexCart.Domain.Enums;
using NSubstitute;
using Xunit;

namespace NexCart.Tests.Services
{
    /// <summary>Administrators sign in only through the admin tool, customers only through the shop.</summary>
    public class UserServiceLoginPortalTests
    {
        private const string Password = "Correct-Horse-9";

        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();

        private UserService CreateSut()
        {
            return new UserService(
                _unitOfWork,
                _userRepo,
                Substitute.For<IOrderRepository>(),
                Substitute.For<IPaymentRepository>(),
                Substitute.For<IProductRepository>(),
                Substitute.For<IShoppingCartRepository>(),
                Substitute.For<IShoppingCartDetailRepository>(),
                new JwtSettings { Key = new string('k', 64) });
        }

        private UserLoginViewModel GivenUser(UserRole role)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = role == UserRole.Admin ? "admin" : "customer",
                Password = PasswordHasher.Hash(Password),
                Role = role
            };

            _userRepo.FilterBy(Arg.Any<Expression<Func<User, bool>>>())
                .Returns(call => new[] { user }.AsQueryable().Where(call.Arg<Expression<Func<User, bool>>>()));

            return new UserLoginViewModel { Username = user.Username, Password = Password };
        }

        [Fact]
        public void Shop_login_accepts_a_customer()
        {
            var result = CreateSut().Login(GivenUser(UserRole.User), LoginPortal.Shop);

            Assert.False(string.IsNullOrEmpty(result.Token));
            Assert.Equal("User", result.Role);
        }

        [Fact]
        public void Shop_login_refuses_an_administrator()
        {
            var login = GivenUser(UserRole.Admin);

            var exception = Assert.Throws<BllException>(() => CreateSut().Login(login, LoginPortal.Shop));

            Assert.Equal(ShopExceptionMassages.UserExceptionMassages.ADMIN_MUST_USE_ADMIN_TOOL, exception.Message);
        }

        [Fact]
        public void Admin_login_accepts_an_administrator()
        {
            var result = CreateSut().Login(GivenUser(UserRole.Admin), LoginPortal.Admin);

            Assert.False(string.IsNullOrEmpty(result.Token));
            Assert.Equal("Admin", result.Role);
        }

        [Fact]
        public void Admin_login_refuses_a_customer()
        {
            var login = GivenUser(UserRole.User);

            var exception = Assert.Throws<BllException>(() => CreateSut().Login(login, LoginPortal.Admin));

            Assert.Equal(ShopExceptionMassages.UserExceptionMassages.ADMIN_ACCESS_REQUIRED, exception.Message);
        }

        [Theory]
        [InlineData(LoginPortal.Shop)]
        [InlineData(LoginPortal.Admin)]
        public void Wrong_password_gives_the_generic_error_on_both_portals(LoginPortal portal)
        {
            // The portal check must not reveal an account's role to someone without its password.
            var login = GivenUser(portal == LoginPortal.Shop ? UserRole.Admin : UserRole.User);
            login.Password = "wrong-password";

            var exception = Assert.Throws<BllException>(() => CreateSut().Login(login, portal));

            Assert.Equal(ShopExceptionMassages.UserExceptionMassages.USERNAME_PASSWORD_INCORRECT, exception.Message);
        }
    }
}
