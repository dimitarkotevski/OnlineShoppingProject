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
                .Not.Nullable();
            Map(o => o.PaymentType)
                .CustomType<PaymentType>()
                .Not.Nullable();
            Map(o => o.TotalAmount)
                .Not.Nullable()
                .Precision(18)
                .Scale(2);
            Map(o => o.DateCreated)
                .Not.Nullable();
            Map(o => o.OrderName)
                .Not.Nullable();

            HasMany(o => o.OrderItems)
                .Inverse()
                .Cascade.AllDeleteOrphan()
                .KeyColumn("OrderId")
                .LazyLoad();

            // The cart is kept for traceability only. What the customer bought lives in
            // OrderItems, so deleting the cart must not delete or rewrite the order.
            References(x => x.ShoppingCart)
                .Column("ShoppingCartId")
                .Nullable()
                .LazyLoad();
            References(x => x.User)
                .Column("UserId")
                .Not.Nullable()
                .LazyLoad();
        }
    }
}
