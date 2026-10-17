using System;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NexCart.Data.NHibernate.Mapping;
using NHibernate;
using NHibernate.Driver;

namespace NexCart.Data.NHibernate.ConfigurationRepository
{
    public class NHibernateContext
    {
        /// <summary>
        /// Builds the session factory for the supplied connection string. The value comes from
        /// application configuration (never hard-coded in source).
        /// </summary>
        public static ISessionFactory BuildSessionFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("A database connection string is required.", nameof(connectionString));
            }

            var build= Fluently.Configure()
             .Database(MsSqlConfiguration.MsSql2005
                .ConnectionString(connectionString)
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
