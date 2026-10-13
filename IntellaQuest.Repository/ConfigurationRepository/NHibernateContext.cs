using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using IntellaQuest.Data.NHibernate.Mapping;
using NHibernate;
using NHibernate.Driver;

namespace IntellaQuest.Data.NHibernate.ConfigurationRepository
{
    public class NHibernateContext
    {
        private static string _connectionString = "Data Source=localhost;Initial Catalog=ShopDb;Integrated Security=SSPI;TrustServerCertificate=True;";
        public static ISessionFactory BuildSessionFactory()
        {
            var build= Fluently.Configure()
             .Database(MsSqlConfiguration.MsSql2005
                .ConnectionString(_connectionString)
                // System.Data.SqlClient is not available on .NET (Core); use the modern driver.
                .Driver<MicrosoftDataSqlClientDriver>())
             .Mappings(m =>
             {
                 m.FluentMappings
                    .AddFromAssembly(typeof(CategoryMap).Assembly);
             })
             .BuildSessionFactory();
            return build;
        }
    }
}
