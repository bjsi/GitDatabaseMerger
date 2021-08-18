using System.Linq;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Data
{
    public interface IGenericRepository<TEntity>
        where TEntity : class
    {
        IQueryable<TEntity> GetAll();
        Task<TEntity> FindByKeysAsync(params object[] keys);
        Task<bool> CreateAsync(TEntity entity);
        Task<bool> UpdateAsync(TEntity entity);
        Task<bool> DeleteAsync(params object[] keys);
    }
}
