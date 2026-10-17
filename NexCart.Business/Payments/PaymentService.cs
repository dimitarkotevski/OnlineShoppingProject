using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Data.NHibernate.Repositories;

namespace NexCart.Business.Services
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
