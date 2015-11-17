using System.Data.Entity.Infrastructure;

namespace DataAccessorDemo.Model.Context
{
    public class DataAccessorContextFactory : IDbContextFactory<DataAccessorContext>
    {
        public DataAccessorContext Create()
        {
            return new DataAccessorContext
            {
                Configuration =
                {
                    ProxyCreationEnabled = false,
                    AutoDetectChangesEnabled = true,
                    LazyLoadingEnabled = false
                }
            };
        }
    }
}