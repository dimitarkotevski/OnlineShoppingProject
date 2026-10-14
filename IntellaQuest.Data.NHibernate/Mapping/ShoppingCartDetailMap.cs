using FluentNHibernate.Mapping;
using IntellaQuest.Domain;

namespace IntellaQuest.Data.NHibernate.Mapping
{
    public class ShoppingCartDetailMap : ClassMap<ShoppingCartDetail>
    {
        public ShoppingCartDetailMap()
        {
            Schema("dbo");
            Table("ShoppingCartDetails");

            Id(c => c.Id);

            Map(c => c.Quantity);

            References(x => x.ShoppingCart)
                .Column("ShoppingCartId").Access
                .CamelCaseField(Prefix.Underscore);

            References(x => x.Product)
                .Column("ProductId").Access
                .CamelCaseField(Prefix.Underscore);

        }
    }
}
