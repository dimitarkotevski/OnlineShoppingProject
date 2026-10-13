using IntellaQuest.Data.NHibernate.ConfigurationRepository;
using IntellaQuest.Domain;

namespace IntellaQuest.Repository.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
    }
    public class OrderRepository(IUnitOfWork unitOfWork) : Repository<Order>(unitOfWork), IOrderRepository
    {
    }
}
