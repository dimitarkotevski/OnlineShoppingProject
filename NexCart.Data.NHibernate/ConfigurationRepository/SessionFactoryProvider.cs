using System;
using System.Threading;
using NHibernate;

namespace NexCart.Data.NHibernate.ConfigurationRepository
{
    public interface ISessionFactoryProvider
    {
        ISessionFactory Get();
    }

    /// <summary>
    /// Builds the NHibernate session factory exactly once and reuses it for the lifetime of the
    /// application. Building a factory (and opening a connection) per request was both slow and
    /// a resource-exhaustion risk.
    /// </summary>
    public sealed class SessionFactoryProvider : ISessionFactoryProvider
    {
        private readonly Lazy<ISessionFactory> _sessionFactory;

        public SessionFactoryProvider(string connectionString)
        {
            _sessionFactory = new Lazy<ISessionFactory>(
                () => NHibernateContext.BuildSessionFactory(connectionString),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public ISessionFactory Get() => _sessionFactory.Value;
    }
}
