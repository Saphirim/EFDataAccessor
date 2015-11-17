using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EFDataAccessor.EFDataAccessor.Model;

namespace EFDataAccessor.EFDataAccessor.Accessor
{
    public interface IDataAccessor : IDisposable
    {
        /// <summary>
        /// Returns a base query with no filters or restrictions applied, that can be used while the DataAccessor is not disposed.
        /// </summary>
        /// <typeparam name="TEntity">Type of the underlying DbSet</typeparam>
        /// <returns>An IQueryable with no limitations</returns>
        IQueryable<TEntity> Set<TEntity>() where TEntity : class, IObjectStateEntity;

        /// <summary>
        /// Returns a base query with no filters or restrictions applied, that can be used while the DataAccessor is not disposed.
        /// </summary>
        /// <typeparam name="TEntity">Type of the underlying DbSet</typeparam>
        /// <param name="includedProperties">Navigation properties to auto include from the base entity</param>
        /// <returns>An IQueryable with no limitations</returns>
        IQueryable<TEntity> Set<TEntity>(params Expression<Func<TEntity, object>>[] includedProperties)
            where TEntity : class, IObjectStateEntity;

        /// <summary>
        /// A Function which returns multiple matching entities of type T as Task of IList
        /// </summary>
        /// <param name="filter">The query filter (criteria)</param>
        /// <param name="includedProperties">Navigation properties to include from the base entity</param>
        /// <returns>An awaitable Task of a collection of matching entities</returns>
        Task<IList<T>> GetEntitiesAsync<T>(Expression<Func<T, bool>> filter,
            params Expression<Func<T, object>>[] includedProperties) where T : class, IObjectStateEntity;

        /// <summary>
        /// Returns exactly the one matching entity of type T as Task of T
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="includedProperties"></param>
        /// <returns>An awaitable Task of the matching T</returns>
        Task<TEntity> GetSingleAsync<TEntity>(Expression<Func<TEntity, bool>> filter,
            params Expression<Func<TEntity, object>>[] includedProperties) where TEntity : class, IObjectStateEntity;

        /// <summary>
        /// Returns exactly the one matching entity of type T as Task of T or null if no matching Entity was found
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="includedProperties"></param>
        /// <returns>An awaitable Task of the matching T</returns>
        Task<TEntity> GetSingleOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> filter,
            params Expression<Func<TEntity, object>>[] includedProperties) where TEntity : class, IObjectStateEntity;

        /// <summary>
        /// Commits pending data changes to the database.
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Inserts or Updates the given Entities in the DataContext.
        /// </summary>
        /// <param name="entities"></param>
        void InsertOrUpdate<TEntity>(params TEntity[] entities) where TEntity : class, IObjectStateEntity;

        /// <summary>
        /// Allows for the modification of a many-to-many relationship
        /// </summary>
        /// <typeparam name="TParent">The type of children entities</typeparam>
        /// /// <typeparam name="TChild">The type of children entities</typeparam>
        /// <param name="parent">The parent entity of type TParent</param>
        /// <param name="collection">The navigation collection of entity types</param>
        /// <param name="state">The entity state to change to (Added or Deleted) </param>
        /// <param name="children">A collection of children of type TChild</param>
        void ModifyRelatedEntities<TParent, TChild>(TParent parent, Expression<Func<TParent, object>> collection,
            EntityState state, params TChild[] children) where TParent : class, IObjectStateEntity where TChild : class, IObjectStateEntity;

        /// <summary>
        /// Sets an entity or entities to be removed on the next SaveChanges call
        /// </summary>        
        /// <param name="entities">Entities to delete</param>       
        void Delete<TEntity>(params TEntity[] entities) where TEntity : class, IObjectStateEntity;

        void ResetDataAccessor();

        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}