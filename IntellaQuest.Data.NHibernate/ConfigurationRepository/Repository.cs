using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IntellaQuest.Data.NHibernate.ConfigurationRepository
{
    public interface IRepository<T> where T : class
    {
        bool Add(T entity);
        bool Add(IEnumerable<T> items);
        bool Update(T entity);
        bool Update(IEnumerable<T> items);
        bool Delete(T entity);
        bool Delete(Guid id);
        bool Delete(IEnumerable<T> entities);
        IQueryable<T> All();
        T FindBy(Guid id);
        T FindBy(Expression<Func<T, bool>> expression);
        IQueryable<T> FilterBy(Expression<Func<T, bool>> expression);
        bool CheckExist(Expression<Func<T, bool>> expression);
    }
    public class Repository<T> : NHibernateContext, IRepository<T> where T : class
    {
        private readonly IUnitOfWork _unitOfWork;
        public Repository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Implementation of IRepository<T>

        public bool Add(T entity)
        {
            _unitOfWork.Session.Save(entity);
            return true;
        }

        public bool Add(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                _unitOfWork.Session.Save(item);
            }

            return true;
        }

        public bool Update(T entity)
        {
            _unitOfWork.Session.Update(entity);

            return true;
        }

        public bool Update(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                _unitOfWork.Session.Update(item);
            }

            return true;
        }

        public bool Delete(T entity)
        {
            _unitOfWork.Session.Delete(entity);

            return true;
        }

        public bool Delete(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
            {
                _unitOfWork.Session.Delete(entity);
            }

            return true;
        }

        public IQueryable<T> All()
        {
            return _unitOfWork.Session.Query<T>();
        }

        public T FindBy(Expression<Func<T, bool>> expression)
        {
            return FilterBy(expression).SingleOrDefault();
        }

        public T FindBy(Guid id)
        {
            return _unitOfWork.Session.Get<T>(id);
        }

        public IQueryable<T> FilterBy(Expression<Func<T, bool>> expression)
        {
            return All().Where(expression).AsQueryable();
        }

        public bool Delete(Guid id)
        {
            var entity = _unitOfWork.Session.Get<T>(id);
            _unitOfWork.Session.Delete(entity);
            return true;
        }

        public bool CheckExist(Expression<Func<T, bool>> expression)
        {
            return All().Where(expression).Any();
        }


        #endregion
    }
}
