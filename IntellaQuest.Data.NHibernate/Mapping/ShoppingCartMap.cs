using FluentNHibernate.Mapping;
using IntellaQuest.Domain;

namespace IntellaQuest.Data.NHibernate.Mapping
{
    public class ShoppingCartMap : ClassMap<ShoppingCart>
    {
        public ShoppingCartMap() 
        {
            Schema("dbo");
            Table("ShoppingCarts");

            Id(c => c.Id);

            Map(c => c.Name);
            Map(c => c.Active);

            HasMany(x => x.ShoppingCartDetails)
                .Inverse()
                .Cascade.AllDeleteOrphan()
                .KeyColumn("ShoppingCartId").LazyLoad();

            References(x => x.User)
                .Column("UserId")
                .Access
                .CamelCaseField(Prefix.Underscore).LazyLoad();

        }
    }
}
