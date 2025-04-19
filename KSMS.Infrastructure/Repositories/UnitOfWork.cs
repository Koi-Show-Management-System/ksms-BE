using KSMS.Application.Repositories;
using KSMS.Domain.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSMS.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace KSMS.Infrastructure.Repositories
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        public TContext Context { get; }
        private Dictionary<Type, object> _repositories;

        public UnitOfWork(TContext context)
        {
            Context = context;
        }

        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            _repositories ??= new Dictionary<Type, object>();
            if (_repositories.TryGetValue(typeof(TEntity), out object repository))
            {
                return (IGenericRepository<TEntity>)repository;
            }

            repository = new GenericRepository<TEntity>(Context);
            _repositories.Add(typeof(TEntity), repository);
            return (IGenericRepository<TEntity>)repository;
        }

        public void Dispose()
        {
            Context?.Dispose();
        }

        public int Commit()
        {
            TrackChanges();
            UpdateEntites();
            return Context.SaveChanges();
        }

        public async Task<int> CommitAsync()
        {
            TrackChanges();
            UpdateEntites();
            return await Context.SaveChangesAsync();
        }

        private void TrackChanges()
        {
            var validationErrors = Context.ChangeTracker.Entries<IValidatableObject>()
                .SelectMany(e => e.Entity.Validate(null))
                .Where(e => e != ValidationResult.Success)
                .ToArray();
            if (validationErrors.Any())
            {
                var exceptionMessage = string.Join(Environment.NewLine,
                    validationErrors.Select(error => $"Properties {error.MemberNames} Error: {error.ErrorMessage}"));
                throw new Exception(exceptionMessage);
            }
        }

        private void UpdateEntites()
        {
            var entries = Context.ChangeTracker.Entries<BaseEntity>();

            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    entityEntry.Property(x => x.CreatedAt)
                        .CurrentValue = VietNamTimeUtil.GetVietnamTime();
                }

                if (entityEntry.State == EntityState.Modified)
                {
                    // Chỉ lấy các scalar properties bị thay đổi (không bao gồm navigation properties)
                    var modifiedProperties = entityEntry.Properties
                        .Where(p => p.IsModified 
                                    && p.Metadata.ClrType.IsPrimitive  // Chỉ lấy thuộc tính kiểu đơn giản
                                    && p.Metadata.Name != nameof(BaseEntity.UpdatedAt))
                        .ToList();

                    // Nếu có thay đổi trong scalar properties, cập nhật UpdatedAt
                    if (modifiedProperties.Any())
                    {
                        entityEntry.Property(x => x.UpdatedAt)
                            .CurrentValue = VietNamTimeUtil.GetVietnamTime();
                    }
                }
            }
        }
        public void Detach<T>(T entity) where T : class
        {
            var entry = Context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                return;
            }
            entry.State = EntityState.Detached;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await Context.Database.BeginTransactionAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            return await Context.Database.BeginTransactionAsync(isolationLevel);
        }
    }
}
