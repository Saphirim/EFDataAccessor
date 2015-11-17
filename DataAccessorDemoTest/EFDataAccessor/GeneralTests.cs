using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using DataAccessorDemo.Model.Context;
using DataAccessorDemo.Model.Entity;
using DataAccessorDemoTest.Helper;
using EFDataAccessor.EFDataAccessor.Accessor;
using EFDataAccessor.EFDataAccessor.Accessor.Impl;
using EFDataAccessor.EFDataAccessor.Factory.Impl;
using EFDataAccessor.EFDataAccessor.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccessorDemoTest.EFDataAccessor
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für GeneralTests
    /// </summary>
    [TestClass]
    public class GeneralTests
    {
        private IDbContextFactory<DbContext> _dbContextFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            _dbContextFactory = new DataAccessorContextFactory();
        }

        [TestMethod]
        public async Task TestDataAccessorFactory()
        {
            var mEntity = await DatabaseInitializeHelper.CreateSimpleMEntity();

            try
            {
                var dataAccessorFactory = new DataAccessorFactory<DataAccessorContextFactory>(new DataAccessorContextFactory());
                using (var dataAccessor = dataAccessorFactory.Create())
                {
                    Assert.AreEqual(mEntity.Id, (await dataAccessor.GetSingleOrDefaultAsync<MEntity>(mE => mE.Name.Equals(mEntity.Name))).Id);
                }
            }
            catch (Exception)
            {
                Assert.Fail("Something went wrong with this DataAccessorFactory thing...");
            }
        }

        [TestMethod]
        public async Task UseNestedDataAccessors()
        {
            var mEntity = await DatabaseInitializeHelper.CreateSimpleMEntity();
            var nEntity = new NEntity
            {
                Name = "UseNestedDataAccessors - new NEntity",
                ObjectState = EObjectState.Added
            };
            var otherEntity = new OtherEntity
            {
                Name = "UseNestedDataAccessors - new OtherEntity",
                ObjectState = EObjectState.Added,
                NEntity = nEntity,
                NEntityId = nEntity.Id
            };

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccessor.InsertOrUpdate(nEntity);
                dataAccessor.ModifyRelatedEntities(mEntity, mE => mE.NEntities, EntityState.Added, nEntity);
                using (IDataAccessor secondDataAccessor = new DataDataAccessor((DataAccessorBase)dataAccessor))
                {
                    secondDataAccessor.InsertOrUpdate(otherEntity);
                    await secondDataAccessor.SaveChangesAsync();
                }
            }

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                var reloadedMEntity =
                    await
                        dataAccessor.GetSingleAsync<MEntity>(mE => mE.Id.Equals(mEntity.Id), mE => mE.NEntities,
                            mE => mE.NEntities.Select(nE => nE.OtherEntities));
                Assert.IsTrue(reloadedMEntity.NEntities.Any(nE => nE.Id.Equals(nEntity.Id)));
                Assert.AreEqual(1, reloadedMEntity.NEntities.Count);
                Assert.IsTrue(reloadedMEntity.NEntities.First().OtherEntities.Any(oE => oE.Id.Equals(otherEntity.Id)));
            }
        }


    }
}
