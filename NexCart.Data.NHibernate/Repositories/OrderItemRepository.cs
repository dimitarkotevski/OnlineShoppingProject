using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Domain;

namespace NexCart.Data.NHibernate.Repositories
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
    }

    public class OrderItemRepository(IUnitOfWork unitOfWork) : Repository<OrderItem>(unitOfWork), IOrderItemRepository
    {
    }
}
