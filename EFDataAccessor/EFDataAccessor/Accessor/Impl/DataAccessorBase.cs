using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EFDataAccessor.EFDataAccessor.Model;

namespace EFDataAccessor.EFDataAccessor.Accessor.Impl
{
    public abstract class DataAccessorBase : IDataAccessor
    {
        #region Properties

        public bool HasPendingChanges
        {
            get
            {
                return DataContext.ChangeTracker.Entries().Any(e => e.State == EntityState.Added ||
                e.State == EntityState.Deleted ||
                e.State == EntityState.Modified ||
                ((IObjectContextAdapter)DataContext).ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Any()||
                ((IObjectContextAdapter)DataContext).ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Deleted).Any()
                );
            }
        }

        #endregion

        #region Internal Properties

        internal DbContextTransaction Transaction
        {
            get { return _transaction; }
            set { _transaction = value; }
        }

        internal DbContext DataContext
        {
            get { return _context; }
            set
            {
                _context = value;
            }
        }

        #endregion

        #region private Members

        protected IDbContextFactory<DbContext> DbContextFactory;
        private DbContext _context;
        private DbContextTransaction _transaction;
        private bool _disposed;

        #endregion

        #region Constructor

        public DataAccessorBase(IDbContextFactory<DbContext> dbContextFactory)
        {
            DbContextFactory = dbContextFactory;
            _context = DbContextFactory.Create();
            _context.Configuration.LazyLoadingEnabled = false;
            _context.Configuration.ProxyCreationEnabled = false;
            _context.Configuration.AutoDetectChangesEnabled = true;
        }

        public DataAccessorBase(DataAccessorBase existing)
        {
            DbContextFactory = existing.DbContextFactory;
            DataContext = existing.DataContext;
            Transaction = existing.Transaction;
        }

        #endregion

        #region public Methods

        public abstract void ResetDataAccessor();

        /// <summary>
        /// Create a new Tx.  If there is an active Tx, it is disposed of (not committed).
        /// </summary>
        public void BeginTransaction()
        {
            _transaction?.Dispose();
            _transaction = DataContext.Database.BeginTransaction();
        }

        /// <summary>
        /// Commit current Tx
        /// </summary>
        public void CommitTransaction()
        {
            _transaction?.Commit();
        }

        /// <summary>
        /// Rolls back current Tx
        /// </summary>
        public void RollbackTransaction()
        {
            _transaction?.Rollback();
        }

        #endregion

        #region Dispose

        [ExcludeFromCodeCoverage]
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                _context?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [ExcludeFromCodeCoverage]
        ~DataAccessorBase()
        {
            Dispose(false);
        }

        #endregion

        public abstract IQueryable<TEntity> Set<TEntity>() where TEntity : class, IObjectStateEntity;
        public abstract IQueryable<TEntity> Set<TEntity>(params Expression<Func<TEntity, object>>[] includedProperties) where TEntity : class, IObjectStateEntity;
        public abstract Task<IList<T>> GetEntitiesAsync<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includedProperties) where T : class, IObjectStateEntity;
        public abstract Task<TEntity> GetSingleAsync<TEntity>(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] includedProperties) where TEntity : class, IObjectStateEntity;
        public abstract Task<TEntity> GetSingleOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] includedProperties) where TEntity : class, IObjectStateEntity;
        public abstract Task<int> SaveChangesAsync();
        public abstract void InsertOrUpdate<TEntity>(params TEntity[] entities) where TEntity : class, IObjectStateEntity;
        public abstract void ModifyRelatedEntities<TParent, TChild>(TParent parent, Expression<Func<TParent, object>> collection, EntityState state,
            params TChild[] children) where TParent : class, IObjectStateEntity where TChild : class, IObjectStateEntity;
        public abstract void Delete<TEntity>(params TEntity[] entities) where TEntity : class, IObjectStateEntity;
    }
}