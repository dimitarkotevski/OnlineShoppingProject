using FluentNHibernate.Mapping;
using IntellaQuest.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntellaQuest.Data.NHibernate.Mapping
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
