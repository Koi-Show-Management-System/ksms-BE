using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Application.Repositories
{
    public interface IUnitOfWork : IGenericRepositoryFactory, IDisposable
    {
        int Commit();

        Task<int> CommitAsync();
    }

    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        TContext Context { get; }
        Task<IDbContextTransaction> BeginTransactionAsync();
        void Detach<T>(T entity) where T : class;

    }
}
