using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Domain;

namespace NexCart.Data.NHibernate.Repositories
{
    public interface IShoppingCartDetailRepository : IRepository<ShoppingCartDetail>
    {

    }
    public class ShoppingCartDetailRepository(IUnitOfWork unitOfWork) : Repository<ShoppingCartDetail>(unitOfWork), IShoppingCartDetailRepository
    {
    }
}
