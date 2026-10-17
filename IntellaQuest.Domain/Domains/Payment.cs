using IntellaQuest.Domain.Enum;
using System;

namespace IntellaQuest.Domain
{
    public class Payment : BaseEntity
    {
        private string _cardNumber;
        private DateTime _expirationDate;
        private string _cardHolder;
        private string _city;
        private string _billingAdress;
        private string _zipPostalCode;
        private string _country;
        private string _phoneNumber;

        public Payment() 
        {
            _expirationDate = DateTime.Now;
        }

        public virtual string CardNumber
        {
            get => _cardNumber;
            set => _cardNumber = value;
        }
        public virtual DateTime ExpirationDate
        {
            get => _expirationDate;
            set => _expirationDate = value;
        }
        public virtual string CardHolder
        {
            get => _cardHolder;
            set => _cardHolder = value;
        }
        public virtual string City
        {
            get => _city;
            set => _city = value;
        }
        public virtual string BillingAdress
        {
            get => _billingAdress;
            set => _billingAdress = value;
        }
        public virtual string ZipPostalCode
        {
            get => _zipPostalCode;
            set => _zipPostalCode = value;
        }
        public virtual string Country
        {
            get => _country;
            set => _country = value;
        }
        public virtual string PhoneNumber
        {
            get => _phoneNumber;
            set => _phoneNumber = value;
        }

    }
}
