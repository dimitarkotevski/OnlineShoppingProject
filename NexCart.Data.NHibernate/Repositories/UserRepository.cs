using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Domain;
using System.Linq;

namespace NexCart.Data.NHibernate.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
    }
    public class UserRepository(IUnitOfWork unitOfWork) : Repository<User>(unitOfWork), IUserRepository
    {
    }
}
