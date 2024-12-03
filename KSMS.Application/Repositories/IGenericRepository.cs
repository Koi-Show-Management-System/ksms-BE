using KSMS.Domain.Pagination;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Repositories
{
    public interface IGenericRepository<T> : IDisposable where T : class
    {
        Task<T> SingleOrDefaultAsync(
            Expression<Func<T, bool>> predicate = null!,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null!,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null!);

        Task<TResult> SingleOrDefaultAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null!,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null!,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null!);

        Task<ICollection<T>> GetListAsync(
            Expression<Func<T, bool>> predicate = null!,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null!,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null!);

        Task<ICollection<TResult>> GetListAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null!,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null!,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null!);

        Task<IPaginate<T>> GetPagingListAsync(
            Expression<Func<T, bool>> predicate = null!,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null!,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null!,
            int page = 1,
            int size = 10);

        Task<IPaginate<TResult>> GetPagingListAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null!,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null!,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null!,
            int page = 1,
            int size = 10);


        Task<T> InsertAsync(T entity);

        Task InsertRangeAsync(IEnumerable<T> entities);


        void UpdateAsync(T entity);

        void UpdateRange(IEnumerable<T> entities);

        void Detach(T entity);


        void DeleteAsync(T entity);
        void DeleteRangeAsync(IEnumerable<T> entities);
    }
}
