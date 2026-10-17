using System;
using System.Linq;
using System.Linq.Expressions;
using NexCart.Business.Security;
using NexCart.Business.Services;
using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Domain;
using NexCart.Domain.Enums;
using NSubstitute;
using Xunit;

namespace NexCart.Tests.Services
{
    public class UserServiceGetRoleTests
    {
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
                new JwtSettings());
        }

        /// <summary>Makes FilterBy run the service's predicate against an in-memory set of users.</summary>
        private void GivenUsers(params User[] users)
        {
            _userRepo.FilterBy(Arg.Any<Expression<Func<User, bool>>>())
                .Returns(call => users.AsQueryable().Where(call.Arg<Expression<Func<User, bool>>>()));
        }

        [Fact]
        public void GetRole_returns_the_current_role_of_an_existing_user()
        {
            var admin = new User { Id = Guid.NewGuid(), Role = UserRole.Admin };
            var customer = new User { Id = Guid.NewGuid(), Role = UserRole.User };
            GivenUsers(admin, customer);

            var sut = CreateSut();

            Assert.Equal(UserRole.Admin, sut.GetRole(admin.Id));
            Assert.Equal(UserRole.User, sut.GetRole(customer.Id));
        }

        [Fact]
        public void GetRole_returns_null_for_a_user_that_does_not_exist()
        {
            GivenUsers(new User { Id = Guid.NewGuid(), Role = UserRole.Admin });

            Assert.Null(CreateSut().GetRole(Guid.NewGuid()));
        }

        [Fact]
        public void GetRole_returns_null_for_an_empty_id_without_querying()
        {
            Assert.Null(CreateSut().GetRole(Guid.Empty));
            _userRepo.DidNotReceive().FilterBy(Arg.Any<Expression<Func<User, bool>>>());
        }
    }
}
