using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexCart.Data.NHibernate.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        Role GetAdminRole();
        Role GetUserRole();
    }
    public class RoleRepository(IUnitOfWork unitOfWork) : Repository<Role>(unitOfWork), IRoleRepository
    {
        public Role GetAdminRole()
        {
            return All().Where(x => x.Name.ToLower().Contains("admin")).FirstOrDefault();
        }

        public Role GetUserRole()
        {
            return All().Where(x => x.Name.ToLower().Contains("user")).FirstOrDefault();
        }
    }
}
