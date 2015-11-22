using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using EFDataAccessor.EFDataAccessor.Accessor;
using EFDataAccessor.EFDataAccessor.Accessor.Impl;

namespace EFDataAccessor.EFDataAccessor.Factory.Impl
{
    public class DataAccessorFactory<TDbContextFactory> : IDataAccessorFactory where TDbContextFactory : IDbContextFactory<DbContext>
    {
        private readonly IDbContextFactory<DbContext> _dbContextFactory;
        public DataAccessorFactory(TDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public IDataAccessor Create()
        {
            return new DataAccessor(_dbContextFactory);
        }
    }
}