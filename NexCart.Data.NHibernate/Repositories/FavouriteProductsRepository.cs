using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexCart.Data.NHibernate.Repositories
{
    public interface IFavouriteProductsRepository : IRepository<FavouriteProducts>
    {
        IQueryable<FavouriteProducts> GetUserFavouriteProducts(User userId);
    }
    public class FavouriteProductsRepository(IUnitOfWork unitOfWork) : Repository<FavouriteProducts>(unitOfWork), IFavouriteProductsRepository
    {
        public IQueryable<FavouriteProducts> GetUserFavouriteProducts(User user)
        {
            return All().Where(x=>x.User == user).AsQueryable();
        }
    }
}
