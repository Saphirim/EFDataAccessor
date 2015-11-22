using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using DataAccessorDemo.Model.Context;
using DataAccessorDemo.Model.Entity;
using DataAccessorDemoTest.Helper;
using EFDataAccessor.EFDataAccessor.Accessor;
using EFDataAccessor.EFDataAccessor.Accessor.Impl;
using EFDataAccessor.EFDataAccessor.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccessorDemoTest.EFDataAccessor
{
    [TestClass]
    public class DeleteTests
    {
        private IDbContextFactory<DbContext> _dbContextFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            _dbContextFactory = new DataAccessorContextFactory();
        }

        [TestMethod]
        public async Task DeleteSimpleEntity()
        {
            var mEntity = await DatabaseInitializeHelper.CreateSimpleMEntity();
            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                //Sicherstellen, dass die MEntity auch in der DB bekannt ist.
                Assert.IsTrue(await dataAccessor.Set<MEntity>().AnyAsync(mE => mE.Id.Equals(mEntity.Id)));
            }



            //Neuen DataAccessor holen, damit die Operation nicht durch LocalCache verfälscht wird
            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                dataAccessor.Delete(mEntity);
                Assert.IsTrue(dataAccessor.HasPendingChanges, "HasPendingChanges should be true!");
                await dataAccessor.SaveChangesAsync();
                Assert.IsFalse(dataAccessor.HasPendingChanges, "HasPendingChanges should be false directly after Saving!");
            }


            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                //Sicherstellen, dass die MEntity auch in der DB bekannt ist.
                Assert.IsFalse(await dataAccessor.Set<MEntity>().AnyAsync(mE => mE.Id.Equals(mEntity.Id)));
            }

        }

        [TestMethod]
        public async Task DeleteRelatedEntity()
        {
            var mEntity = await DatabaseInitializeHelper.CreateMEntityWithSomeNEntites();
            var deletingNEntity = mEntity.NEntities.First();

            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                dataAccessor.Delete(deletingNEntity);
                Assert.IsTrue(dataAccessor.HasPendingChanges, "HasPendingChanges should be true!");
                await dataAccessor.SaveChangesAsync();
                Assert.IsFalse(dataAccessor.HasPendingChanges, "HasPendingChanges should be false directly after Saving!");
            }

            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                var reloadedMEntity =
                    await dataAccessor.GetSingleAsync<MEntity>(mE => mE.Id.Equals(mEntity.Id), mE => mE.NEntities);
                Assert.AreEqual(1, reloadedMEntity.NEntities.Count);
                Assert.IsFalse(await dataAccessor.Set<MEntity>().AnyAsync(nE => nE.Id.Equals(deletingNEntity.Id)));
            }
        }

        [TestMethod]
        public async Task DeleteOtherEntity()
        {
            var nEntity = await DatabaseInitializeHelper.CreateNEntityWithSomeOtherEntities();
            var deletingOtherEntity = nEntity.OtherEntities.First();

            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                dataAccessor.Delete(deletingOtherEntity);
                Assert.IsTrue(dataAccessor.HasPendingChanges, "HasPendingChanges should be true!");
                await dataAccessor.SaveChangesAsync();
                Assert.IsFalse(dataAccessor.HasPendingChanges, "HasPendingChanges should be false directly after Saving!");
            }
            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                var reloadedNEntity =
                    await dataAccessor.GetSingleAsync<NEntity>(nE => nE.Id.Equals(nEntity.Id), nE => nE.OtherEntities);
                Assert.IsFalse(reloadedNEntity.OtherEntities.Any(oE => oE.Id.Equals(deletingOtherEntity.Id)));
            }
        }

        [TestMethod]
        public async Task CascadingDeleteNEntityWithOtherEntities()
        {
            var nEntity = await DatabaseInitializeHelper.CreateNEntityWithSomeOtherEntities();

            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                dataAccessor.Delete(nEntity);
                Assert.IsTrue(dataAccessor.HasPendingChanges, "HasPendingChanges should be true!");
                await dataAccessor.SaveChangesAsync();
                Assert.IsFalse(dataAccessor.HasPendingChanges, "HasPendingChanges should be false directly after Saving!");
            }
            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                Assert.IsFalse(await dataAccessor.Set<NEntity>().AnyAsync(nE => nE.Id.Equals(nEntity.Id)));
            }
        }

        [TestMethod]
        public async Task MultipleRelationModificationsBeforeSave()
        {
            const string newNEntityName = "MultipleRelationModificationsBeforeSave - newNEntity";
            var mEntity = await DatabaseInitializeHelper.CreateMEntityWithSomeNEntites();
            var existingNEntity = await DatabaseInitializeHelper.CreateNEntityWithSomeOtherEntities();
            var newNEntity = new NEntity
            {
                Name = newNEntityName,
                ObjectState = EObjectState.Added
            };

            //Add both Entities to the ObjectList in the Domainmodel
            mEntity.NEntities.Add(newNEntity);
            mEntity.ObjectState = EObjectState.Modified;

            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                dataAccessor.InsertOrUpdate(mEntity);
                dataAccessor.ModifyRelatedEntities(mEntity, mE => mE.NEntities, EntityState.Added, existingNEntity);
                dataAccessor.ModifyRelatedEntities(mEntity, mE => mE.NEntities, EntityState.Deleted, existingNEntity);
                Assert.IsTrue(dataAccessor.HasPendingChanges, "HasPendingChanges should be true!");
                await dataAccessor.SaveChangesAsync();
                Assert.IsFalse(dataAccessor.HasPendingChanges, "HasPendingChanges should be false directly after Saving!");
            }
            using (IDataAccessor dataAccessor = new DataAccessor(_dbContextFactory))
            {
                var reloadedMEntity = await dataAccessor.GetSingleAsync<MEntity>(mE => mE.Id.Equals(mEntity.Id), mE => mE.NEntities);
                var reloadedNEntity = await dataAccessor.GetSingleAsync<NEntity>(nE => nE.Id.Equals(existingNEntity.Id), nE => nE.MEntities);
                Assert.AreEqual(3, reloadedMEntity.NEntities.Count);
                Assert.AreEqual(0, reloadedNEntity.MEntities.Count);
            }
        }
    }
}
