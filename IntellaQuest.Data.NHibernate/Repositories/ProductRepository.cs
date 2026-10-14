using IntellaQuest.Data.NHibernate.ConfigurationRepository;
using IntellaQuest.Domain;

namespace IntellaQuest.Data.NHibernate.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
    }
    public class ProductRepository(IUnitOfWork unitOfWork) : Repository<Product>(unitOfWork), IProductRepository
    {
    }
}
