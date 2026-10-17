using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Domain;
using System.Linq;

namespace NexCart.Data.NHibernate.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        IQueryable<Product> GetProductsByCategory(Category category);
    }
    public class CategoryRepository(IUnitOfWork unitOfWork) : Repository<Category>(unitOfWork), ICategoryRepository
    {
        public IQueryable<Product> GetProductsByCategory(Category category)
        {
            return All().Where(x => x == category).SelectMany(y=>y.Products);
        }
    }
}
