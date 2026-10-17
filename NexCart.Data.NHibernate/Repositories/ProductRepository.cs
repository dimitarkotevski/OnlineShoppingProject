using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Domain;

namespace NexCart.Data.NHibernate.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
    }
    public class ProductRepository(IUnitOfWork unitOfWork) : Repository<Product>(unitOfWork), IProductRepository
    {
    }
}
