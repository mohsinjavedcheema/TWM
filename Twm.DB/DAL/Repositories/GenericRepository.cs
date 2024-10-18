using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.DB.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Twm.DB.DAL.Repositories
{
    public abstract class GenericRepository<TEntity, TDbContext> : IGenericRepository<TEntity> where TEntity : class where TDbContext : DbContext
    {
        protected readonly TDbContext Context;

        protected readonly DbSet<TEntity> DbSet;

        protected GenericRepository(TDbContext context)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
        }

        public IEnumerable<TEntity> FindBy(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            IEnumerable<TEntity> query = DbSet.Where(predicate).AsEnumerable();
            return query.Cast<TEntity>();
        }

        public async Task<TEntity> GetEntity(int entityId)
        {
            return DbSet.FindAsync(entityId).Result as TEntity;
        }

        public async Task Add(TEntity entity)
        {
            await DbSet.AddAsync(entity);
        }


        public async Task Add(TEntity[] entities)
        {
            await DbSet.AddRangeAsync(entities.Cast<TEntity>());
        }


        public async Task Update(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
            await Context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TEntity>> GetEntities()
        {

            return  DbSet.ToListAsync().Result.Cast<TEntity>();
        }

        public void Remove(TEntity entity)
        {
            DbSet.Remove(entity as TEntity);
        }

        public void Remove(TEntity[] entities)
        {
            DbSet.RemoveRange(entities.Cast<TEntity>());
        }

        public async Task CompleteAsync()
        {
            await Context.SaveChangesAsync();
        }
    }
}