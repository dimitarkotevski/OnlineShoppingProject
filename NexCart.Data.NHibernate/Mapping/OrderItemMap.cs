using FluentNHibernate.Mapping;
using NexCart.Domain;

namespace NexCart.Data.NHibernate.Mapping
{
    public class OrderItemMap : ClassMap<OrderItem>
    {
        public OrderItemMap()
        {
            Schema("dbo");
            Table("OrderItems");

            Id(c => c.Id);

            Map(c => c.ProductName)
                .Column("ProductName")
                .Not.Nullable()
                .Length(255)
                .Access.CamelCaseField(Prefix.Underscore);

            Map(c => c.UnitPrice)
                .Column("UnitPrice")
                .Not.Nullable()
                .Precision(18)
                .Scale(2)
                .Access.CamelCaseField(Prefix.Underscore);

            Map(c => c.Quantity)
                .Column("Quantity")
                .Not.Nullable()
                .Access.CamelCaseField(Prefix.Underscore);

            References(c => c.Order)
                .Column("OrderId")
                .Not.Nullable()
                .Access.CamelCaseField(Prefix.Underscore)
                .LazyLoad();

            References(c => c.Product)
                .Column("ProductId")
                .Nullable()
                .Access.CamelCaseField(Prefix.Underscore)
                .LazyLoad();
        }
    }
}
