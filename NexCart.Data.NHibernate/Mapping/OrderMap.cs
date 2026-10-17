using FluentNHibernate.Mapping;
using NexCart.Domain;
using NexCart.Domain.Enum;
using NexCart.Domain.Enums;

namespace NexCart.Data.NHibernate.Mapping
{
    public class OrderMap : ClassMap<Order>
    {
        public OrderMap()
        {
            Schema("dbo");
            Table("Orders");

            Id(o => o.Id);

            Map(o => o.OrderStatus)
                .Access.CamelCaseField(Prefix.Underscore)
                .CustomType<OrderStatus>()
                .Nullable();
            Map(o => o.PaymentType)
                .CustomType<PaymentType>();
            Map(o => o.TotalAmount);
            Map(o => o.DateCreated);
            Map(o => o.OrderName);

            References(x => x.ShoppingCart)
                .Column("ShoppingCartId")
                .Cascade.SaveUpdate().LazyLoad();
            References(x => x.User)
                 .Column("UserId")
                 .Cascade.SaveUpdate().LazyLoad();
        }

    }
}
