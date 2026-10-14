using FluentNHibernate.Mapping;
using IntellaQuest.Domain;

namespace IntellaQuest.Data.NHibernate.Mapping
{
    public class FavouriteProductsMap : ClassMap<FavouriteProducts>
    {
        public FavouriteProductsMap()
        {
            Schema("dbo");
            Table("Favourite");

            Id(c => c.Id).GeneratedBy.Guid();

            #region Foreign Key

            References(x => x.User)
                            .Column("UserId")
                            .Access
                            .CamelCaseField(Prefix.Underscore).LazyLoad();

            References(x => x.Product)
                .Column("ProductId")
                .Access
                .CamelCaseField(Prefix.Underscore).LazyLoad();

            #endregion
        }
    }
}
