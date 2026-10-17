

using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Domain;

namespace NexCart.Data.NHibernate.Repositories
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
    }
    public class ShoppingCartRepository(IUnitOfWork unitOfWork) : Repository<ShoppingCart>(unitOfWork), IShoppingCartRepository
    {
    }
}
