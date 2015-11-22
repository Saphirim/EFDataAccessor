using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EFDataAccessor.EFDataAccessor.Model;

namespace EFDataAccessor.EFDataAccessor.Accessor.Impl
{
    public class DataAccessor : DataAccessorBase
    {
        #region Constructors

        /// <summary>
        /// Default constructor - passes to the base class for DbContext instantiation
        /// </summary>
        public DataAccessor(IDbContextFactory<DbContext> dbContextFactory) : base(dbContextFactory)
        {
        }

        /// <summary>
        /// Sort of like a copy constructor where we reuse an existing DbContext with a new Data Accessor class
        /// </summary>
        /// <param name="existing">A sort of copy constructor where an existing accessor's DbContext is used by this class instance</param>
        public DataAccessor(DataAccessorBase existing) : base(existing)
        {
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Returns a base query with no filters or restrictions applied, that can be used while the DataAccessor is not disposed.
        /// </summary>
        /// <typeparam name="TEntity">Type of the underlying DbSet</typeparam>
        /// <returns>An IQueryable with no limitations</returns>
        public override IQueryable<TEntity> Set<TEntity>()
        {
            return DataContext.Set<TEntity>().AsQueryable();
        }

        /// <summary>
        /// Returns a base query with no filters or restrictions applied, that can be used while the DataAccessor is not disposed.
        /// </summary>
        /// <typeparam name="TEntity">Type of the underlying DbSet</typeparam>
        /// <param name="includedProperties">Navigation properties to auto include from the base entity</param>
        /// <returns>An IQueryable with no limitations</returns>
        public override IQueryable<TEntity> Set<TEntity>(params Expression<Func<TEntity, object>>[] includedProperties)
        {
            IQueryable<TEntity> query = DataContext.Set<TEntity>();
            query = includedProperties.Aggregate(query, (entity, property) => entity.Include(property));
            return query;
        }

        /// <summary>
        /// A Function which returns multiple matching entities of type T as Task of IList
        /// </summary>
        /// <param name="filter">The query filter (criteria)</param>
        /// <param name="includedProperties">Navigation properties to include from the base entity</param>
        /// <returns>An awaitable Task of a collection of matching entities</returns>
        public override async Task<IList<TEntity>> GetEntitiesAsync<TEntity>(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] includedProperties)
        {
            return await Set(includedProperties).Where(filter).ToListAsync();
        }

        /// <summary>
        /// Returns exactly the one matching entity of type T as Task of T
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="includedProperties"></param>
        /// <returns>An awaitable Task of the matching T</returns>
        public override async Task<TEntity> GetSingleAsync<TEntity>(Expression<Func<TEntity, bool>> filter,
            params Expression<Func<TEntity, object>>[] includedProperties)
        {
            return await Set(includedProperties).Where(filter).SingleAsync();
        }

        public override async Task<TEntity> GetSingleOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] includedProperties)
        {
            return await Set(includedProperties).Where(filter).SingleOrDefaultAsync();
        }

        /// <summary>
        /// Commits pending data changes to the database.
        /// </summary>
        public override async Task<int> SaveChangesAsync()
        {
            var returnCode = -1;
            try
            {
                returnCode = await DataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex, true);
            }
            _resetDataContext();
            return returnCode;
        }

        public override void ResetDataAccessor()
        {
            _resetDataContext();
        }

        /// <summary>
        /// Inserts or Updates the given Entities in the DataContext.
        /// </summary>
        /// <param name="entities"></param>
        public override void InsertOrUpdate<TEntity>(params TEntity[] entities)
        {
            ApplyStateChanges(entities);
        }

        /// <summary>
        /// Allows for the modification of a many-to-many relationship
        /// </summary>
        /// <typeparam name="TParent">The type of children entities</typeparam>
        /// /// <typeparam name="TChild">The type of children entities</typeparam>
        /// <param name="parent">The parent entity of type TParent</param>
        /// <param name="collection">The navigation collection of entity types</param>
        /// <param name="state">The entity state to change to (Added or Deleted) </param>
        /// <param name="children">A collection of children of type TChild</param>
        public override void ModifyRelatedEntities<TParent, TChild>(TParent parent, Expression<Func<TParent, object>> collection, EntityState state, params TChild[] children)
        {
            try
            {
                if (!ExistsLocal(parent))
                {
                    DataContext.Set<TParent>().Attach(parent);
                }
                var obj = ((IObjectContextAdapter)DataContext).ObjectContext;
                foreach (var child in children)
                {
                    if (ExistsLocal(child)) //Try local editing first:
                    {
                        var childCollection = ((ICollection<TChild>)collection.Compile().Invoke(parent));
                        if (state == EntityState.Deleted && childCollection.Contains(child))
                        {
                            childCollection.Remove(child);
                        }
                        else if (state == EntityState.Added && !childCollection.Contains(child))
                        {
                            childCollection.Add(child);
                        }
                    }
                    else
                    {
                        DataContext.Set<TChild>().Attach(child);
                        obj.ObjectStateManager.ChangeRelationshipState(parent, child, collection, state);
                    }
                }
            }
            catch (NotSupportedException ex)
            {
                Debug.WriteLine("An attempt was made to try and modify a relationship which is not supported.  Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Sets an entity or entities to be removed on the next Save Changes call
        /// </summary>        
        /// <param name="entities">Entities to delete</param>        
        public override void Delete<TEntity>(params TEntity[] entities)
        {
            foreach (var entity in entities.Where(x => x.ObjectState != EObjectState.Deleted))
            {
                entity.ObjectState = EObjectState.Deleted;
            }
            ApplyStateChanges(entities);
        }

        #endregion

        #region Protected Functions

        /// <summary>
        /// Determines if the specified entity exists in the *local* Db Context cache
        /// Note: does not check the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected bool ExistsLocal<T>(T entity) where T : class, IObjectStateEntity
        {
            return DataContext.Set<T>().Local.Any(e => e == entity);
        }

        /// <summary>
        /// Determines if an entity exists in the Data Context or in the physical data store
        /// Source (partial): http://stackoverflow.com/questions/6018711/generic-way-to-check-if-entity-exists-in-entity-framework
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns>Returns null if not found, else returns the located entity which may not be the same as the entity passed in</returns>
        protected T Exists<T>(T entity) where T : class, IObjectStateEntity
        {
            var objContext = ((IObjectContextAdapter)DataContext).ObjectContext;
            var objSet = objContext.CreateObjectSet<T>();
            var entityKey = objContext.CreateEntityKey(objSet.EntitySet.Name, entity);

            var set = DataContext.Set<T>();
            var keys = (from x in entityKey.EntityKeyValues
                        select x.Value).ToArray();

            //Remember, there can by surrogate keys, so don't assume there's just one column/one value
            //If a surrogate key isn't ordered properly, the Set<T>().Find() method will fail, use attributes on the entity to determien the proper order.

            //context.Configuration.AutoDetectChangesEnabled = false;
            //http://stackoverflow.com/questions/11686225/dbset-find-method-ridiculously-slow-compared-to-singleordefault-on-id

            return set.Find(keys);
        }



        #endregion

        #region Private Functions

        /// <summary>
        /// Loops through entities and determines if they need to be attached, added and then sets the entity state accordingly
        /// </summary>
        /// <typeparam name="TEntity">Type of items to process</typeparam>
        /// <param name="items">A collection of entities to process</param>
        private void ApplyStateChanges<TEntity>(params TEntity[] items) where TEntity : class, IObjectStateEntity
        {
            var results = new List<ValidationResult>();
            foreach (var item in items)
            {
                Validator.TryValidateObject(item, new ValidationContext(item, null, null), results, true);
            }
            if (results.Any())
            {
                var errorMessages = results.Select(result => result.ErrorMessage).Aggregate((a, b) => a + ", " + b);
                throw new ValidationException("Can not apply current changes, validation has failed for one or more entities: " + errorMessages);
            }

            try
            {
                Debug.WriteLine("Started ApplyStateChanges");

                var dbSet = DataContext.Set<TEntity>();

                //ignore anything previously handled
                foreach (var item in items.Where(x => x.ObjectState != EObjectState.Processed))
                {
                    Debug.WriteLine("Item: " + item.ObjectState + item.GetType());

                    ProcessEntityState(dbSet, item);

                    foreach (var entry in DataContext.ChangeTracker.Entries<IObjectStateEntity>()
                        .Where(c => c.Entity.ObjectState != EObjectState.Processed))
                    {
                        Debug.WriteLine("Entry: " + entry.Entity.ObjectState + entry.Entity.GetType());

                        var y = DataContext.Entry(entry.Entity);
                        y.State = HelperFunctions.ConvertState(entry.Entity.ObjectState);

                        entry.Entity.ObjectState = EObjectState.Processed;
                    }
                }
                Debug.WriteLine("Finished ApplyStateChanges");
            }
            catch (Exception ex)
            {
                HandleException(ex, true);
            }
        }

        /// <summary>
        /// This function checks the state of the entity (of type T2) and handles it accordingly
        /// Note: refactored out of the ApplyStateChanges function, for readability
        /// </summary>
        /// <typeparam name="T2">Type of the parameter arguments</typeparam>
        /// <param name="dbSet">The DataContext set of type (T2)</param>
        /// <param name="item">The item of type (T2) to process</param>
        private void ProcessEntityState<T2>(DbSet<T2> dbSet, T2 item) where T2 : class, IObjectStateEntity
        {
            DbEntityEntry itemEntry = DataContext.Entry(item);
            // ignore attached entities, only process detached entities
            if (itemEntry.State != EntityState.Detached)
            {
                return;
            }
            switch (item.ObjectState)
            {
                case EObjectState.Added:
                    {
                        dbSet.Add(item);
                        break;
                    }

                case EObjectState.Modified:
                    {
                        if (!ExistsLocal(item))
                        {
                            dbSet.Attach(item);
                        }
                        break;
                    }
                case EObjectState.Deleted:
                    {
                        //we need to ensure the item actually exists, by loading it explicitly
                        var existing = Exists(item);
                        if (existing != null)
                        {
                            existing.ObjectState = item.ObjectState;
                            dbSet.Remove(existing);
                        }
                        break;
                    }
                    //if it doesn't exist, we can't delete it
            }
        }

        // ReSharper disable once UnusedParameter.Local
        // Parameter is used, misinterpreting of Resharper
        [ExcludeFromCodeCoverage]
        private static void HandleException(Exception ex, bool rethrow = false)
        {
            Trace.WriteLine(ex.Message);
            if (ex.InnerException != null)
            {
                Trace.WriteLine(ex.InnerException.Message);
            }
            if (rethrow)
            {
                throw ex;
            }
        }

        private void _resetDataContext()
        {
            DataContext.Dispose();
            DataContext = DbContextFactory.Create();
        }

        #endregion
    }
}