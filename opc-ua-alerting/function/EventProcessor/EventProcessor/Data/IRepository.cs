using System.Linq;
using System.Threading.Tasks;

namespace EventProcessor.Data
{
    public interface IRepository<T> where T : Entity
    {
        IQueryable<T> All { get; }

        Task AddAsync(T entity);

        Task UpsertAsync(T entity);

        Task DeleteAsync(T entity);
    }
}
