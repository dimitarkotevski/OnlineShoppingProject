using IntellaQuest.Data.NHibernate.ConfigurationRepository;
using IntellaQuest.Domain;

namespace IntellaQuest.Data.NHibernate.Repositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {

    }
    public class PaymentRepository(IUnitOfWork unitOfWork) : Repository<Payment>(unitOfWork), IPaymentRepository
    {
    }
}
