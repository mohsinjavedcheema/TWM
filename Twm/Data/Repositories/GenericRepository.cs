using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlgoDesk.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDesk.Data.Repositories
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
            return query;
        }

        public async Task<TEntity> GetEntity(int entityId)
        {
            return await DbSet.FindAsync(entityId);
        }

        public async Task Add(TEntity entity)
        {
            await DbSet.AddAsync(entity);
        }


        public async Task Add(TEntity[] entities)
        {
            await DbSet.AddRangeAsync(entities);
        }


        public async Task Update(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
            await Context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TEntity>> GetEntities()
        {

            return await DbSet.ToListAsync();
        }

        public void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public void Remove(TEntity[] entities)
        {
            DbSet.RemoveRange(entities);
        }

        public async Task CompleteAsync()
        {
            await Context.SaveChangesAsync();
        }
    }
}