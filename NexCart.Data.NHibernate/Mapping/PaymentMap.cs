using FluentNHibernate.Mapping;
using NexCart.Domain;
using NexCart.Domain.Enum;

namespace NexCart.Data.NHibernate.Mapping
{
    public class PaymentMap : ClassMap<Payment>
    {
        public PaymentMap() 
        {
            Schema("dbo");
            Table("Payments");

            Id(p => p.Id);

            #region Attribute

            // NOTE: card security codes (CVV) are deliberately not mapped, so they are never
            // persisted. Only the card holder and the last four digits of the number are stored.
            Map(p => p.CardNumber);
            Map(p => p.ExpirationDate);
            Map(p => p.CardHolder);
            Map(p => p.City);
            Map(p => p.BillingAdress);
            Map(p => p.ZipPostalCode);
            Map(p => p.Country);
            Map(p => p.PhoneNumber);

            #endregion

        }
    }
}
