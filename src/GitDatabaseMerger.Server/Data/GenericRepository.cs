using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Data
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private DbContext _dbContext { get; }

        private ConcurrentDictionary<Type, string[]> PrimaryKeyPropertiesByEntityType { get; } = new ConcurrentDictionary<Type, string[]>();

        public GenericRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public object[] KeysOf(TEntity entity)
        {
            var entry = _dbContext.Entry(entity);
            var keyProperties = PrimaryKeyPropertiesByEntityType.GetOrAdd(
                entity.GetType(),
                t => entry.Metadata.FindPrimaryKey().Properties
                                   .Select(property => property.Name)
                                   .ToArray());

            var keyParts = keyProperties
                .Select(propertyName => entry.Property(propertyName).CurrentValue)
                .ToArray();

            return keyParts;
        }

        public async Task<bool> CreateAsync(TEntity entity)
        {
            try
            {
                await _dbContext.Set<TEntity>().AddAsync(entity);
                return (await _dbContext.SaveChangesAsync()) == 1;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(TEntity entity)
        {
            try
            {
                _dbContext.Set<TEntity>().Remove(entity);
                return (await _dbContext.SaveChangesAsync()) == 1;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public IQueryable<TEntity> GetAll()
        {
            return _dbContext.Set<TEntity>().AsNoTracking();
        }

        public async Task<TEntity> FindByKeysAsync(params object[] keys)
        {
            return await _dbContext.Set<TEntity>()
                .FindAsync(keys);
        }

        public async Task<bool> UpdateAsync(TEntity entity)
        {
            try
            {
                _dbContext.Set<TEntity>().Update(entity);
                return (await _dbContext.SaveChangesAsync()) == 1;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
    }
}
