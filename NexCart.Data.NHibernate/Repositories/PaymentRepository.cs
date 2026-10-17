using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Domain;

namespace NexCart.Data.NHibernate.Repositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {

    }
    public class PaymentRepository(IUnitOfWork unitOfWork) : Repository<Payment>(unitOfWork), IPaymentRepository
    {
    }
}
