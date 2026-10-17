using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Domain;

namespace NexCart.Data.NHibernate.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
    }
    public class OrderRepository(IUnitOfWork unitOfWork) : Repository<Order>(unitOfWork), IOrderRepository
    {
    }
}
