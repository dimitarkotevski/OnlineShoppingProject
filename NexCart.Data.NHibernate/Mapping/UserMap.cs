using FluentNHibernate.Mapping;
using NexCart.Domain;
using NexCart.Domain.Enums;

namespace NexCart.Data.NHibernate.Mapping
{
    public class UserMap : ClassMap<User>
    {
        public UserMap() {
            Schema("dbo");
            Table("Users");

            Id(c => c.Id); // .Colum("UserId")

            #region Attributes

            Map(c => c.FirstName) // _firstName
                .Column("FirstName")
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();
            Map(c => c.LastName)
                .Column("LastName")
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();
            Map(c => c.Email)
                .Column("Email")
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();
            Map(c => c.Username)
                .Column("Username")
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();
            Map(c => c.Password)
                .Column("Password")
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();

            Map(c => c.Description)
                .Column("Description")
                .Access.CamelCaseField(Prefix.Underscore)
                .Nullable();

            Map(c => c.Address)
                .Column("Address")
                .Access.CamelCaseField(Prefix.Underscore)
                .Nullable();

            Map(c => c.State)
                .Column("State")
                .Access.CamelCaseField(Prefix.Underscore)
                .Nullable();

            Map(c => c.City)
                .Column("City")
                .Access.CamelCaseField(Prefix.Underscore)
                .Nullable();

            Map(c => c.ZipCode)
                .Column("ZipCode")
                .Access.CamelCaseField(Prefix.Underscore)
                .Nullable();

            Map(c => c.Amount)
                .Column("Amount")
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
                .Precision(18)
                .Scale(2);

            #endregion

            #region Foreign Key


            References(x => x.Payment)
                .Column("PaymentId")
                .Access
                .CamelCaseField(Prefix.Underscore).Cascade.All();

            // Stored as an INT (NexCart.Domain.Enums.UserRole). Every user must have a role;
            // the migrations default Users.Role to the least-privileged one.
            Map(x => x.Role)
                .Column("Role")
                .CustomType<UserRole>()
                .Not.Nullable()
                .Access
                .CamelCaseField(Prefix.Underscore);

            HasMany(x => x.Orders)
                .Inverse()
                .Cascade.AllDeleteOrphan()
                .KeyColumn("UserId").LazyLoad();


            HasManyToMany(x => x.FavouriteProducts).Schema("dbo").Table("Favourite")
               .ParentKeyColumn("UserId")
               .ChildKeyColumn("ProductId")
               .Access.CamelCaseField(Prefix.Underscore);

            #endregion

        }
    }
}
