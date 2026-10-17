using FluentNHibernate.Mapping;
using NexCart.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexCart.Data.NHibernate.Mapping
{
    public class RoleMap : ClassMap<Role>
    {
        public RoleMap() 
        {
            Schema("dbo");
            Table("Roles");

            Id(c => c.Id);

            Map(c => c.Name)
                .Column("Name")
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable();
        }
    }
}
