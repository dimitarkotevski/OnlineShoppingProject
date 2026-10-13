

using IntellaQuest.Data.NHibernate.ConfigurationRepository;
using IntellaQuest.Domain;

namespace IntellaQuest.Data.NHibernate.Repositories
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
    }
    public class ShoppingCartRepository(IUnitOfWork unitOfWork) : Repository<ShoppingCart>(unitOfWork), IShoppingCartRepository
    {
    }
}
