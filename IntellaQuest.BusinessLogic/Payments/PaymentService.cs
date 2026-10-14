using IntellaQuest.Data.NHibernate.ConfigurationRepository;
using IntellaQuest.Data.NHibernate.Repositories;

namespace IntellaQuest.BusinessLogic.Services
{
    public interface IPaymentService
    {

    }
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPaymentRepository _paymentRepository;
        public PaymentService(IPaymentRepository paymentRepository, IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }
    }
}
