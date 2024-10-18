using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AlgoDesk.Data.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetEntity(int entityId);

        Task Add(TEntity entity);

        Task Add(TEntity[] entities);

        Task Update(TEntity entity);

        Task<IEnumerable<TEntity>> GetEntities();

        IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate);

        void Remove(TEntity entity);

        void Remove(TEntity[] entities);

        Task CompleteAsync();
    }
}