using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using DataAccessorDemo.Model.Context;
using DataAccessorDemo.Model.Entity;
using DataAccessorDemoTest.Helper;
using EFDataAccessor.EFDataAccessor.Accessor;
using EFDataAccessor.EFDataAccessor.Accessor.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccessorDemoTest.EFDataAccessor
{
    [TestClass]
    public class ReadTests
    {
        private IDbContextFactory<DbContext> _dbContextFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            _dbContextFactory = new DataAccessorContextFactory();
        }

        [TestMethod]
        public async Task SetWithAutoIncludedNavigationProperties()
        {
            var mEntity = await DatabaseInitializeHelper.CreateMEntityWithSomeNEntites();
            var nEntity = await DatabaseInitializeHelper.CreateNEntityWithSomeOtherEntities();

            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                dataAccessor.ModifyRelatedEntities(mEntity, mE => mE.NEntities, EntityState.Added, nEntity);
                Assert.IsTrue(dataAccessor.HasPendingChanges, "HasPendingChanges should be true after Modifying the Model!");
                await dataAccessor.SaveChangesAsync();
                Assert.IsFalse(dataAccessor.HasPendingChanges, "HasPendingChanges should be false directly after Saving!");
            }

            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                var reloadedMEntity = await dataAccessor.Set<MEntity>(mE => mE.NEntities, mE => mE.NEntities.Select(nE => nE.OtherEntities))
                    .SingleAsync(mE => mE.Id.Equals(mEntity.Id));
                Assert.IsTrue(reloadedMEntity.NEntities.Any(nE => nE.OtherEntities.Count > 0));
            }
        }

        [TestMethod]
        public async Task GetEntitiesAsync()
        {
            var mEntity1 = await DatabaseInitializeHelper.CreateMEntityWithSomeNEntites();
            var mEntity2 = await DatabaseInitializeHelper.CreateMEntityWithSomeNEntites();
            var mEntity3 = await DatabaseInitializeHelper.CreateMEntityWithSomeNEntites();

            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                var mEntities = await dataAccessor.GetEntitiesAsync<MEntity>(mE => mE.Id.Equals(mEntity1.Id) || mE.Id.Equals(mEntity2.Id),
                    mE => mE.NEntities);
                Assert.AreEqual(2, mEntities.Count);
                Assert.IsTrue(mEntities.Any(mE => mE.Id.Equals(mEntity1.Id)));
                Assert.IsTrue(mEntities.Any(mE => mE.Id.Equals(mEntity2.Id)));
            }

        }
    }
}
