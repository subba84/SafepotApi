using System.Linq.Expressions;

namespace Safepot.Contracts
{
    public interface ISfpDataRepository<T>
    {
        Task<IEnumerable<T>> GetAsync(Expression<Func<T,bool>>? filter = null, Func<IQueryable<T>,IOrderedQueryable<T>>? orderBy = null);
        Task<int> CreateAsync(T entity);
        Task CreateRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteAllAsync(IEnumerable<T> entities);
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
        IQueryable<T> FindBy(Expression<Func<T, bool>> predicate);
        Dictionary<string, string>? GetParameterDecryptValues(Dictionary<string, string> parameters);
        Task ClearData();
    }
}