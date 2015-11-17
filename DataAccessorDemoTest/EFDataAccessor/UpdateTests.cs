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
    public class UpdateTests
    {
        private IDbContextFactory<DbContext> _dbContextFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            _dbContextFactory = new DataAccessorContextFactory();
        }

        [TestMethod]
        public async Task UpdateSimpleEntity()
        {
            const string updatedMEntityName = "UpdateSimpleEntity - updatedName";
            var mEntity = await DatabaseInitializeHelper.CreateSimpleMEntity();

            mEntity.Name = updatedMEntityName;
            mEntity.ObjectState = EObjectState.Modified;

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccessor.InsertOrUpdate(mEntity);
                await dataAccessor.SaveChangesAsync();
            }

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                Assert.IsTrue((await dataAccessor.GetSingleAsync<MEntity>(mE => mE.Id.Equals(mEntity.Id))).Name.Equals(updatedMEntityName));
            }
        }

        [TestMethod]
        public async Task UpdateMEntityAddNewNEntity()
        {
            const string newNEntityName = "UpdateMEntityAddNewNEntity - newNEntity";
            var mEntity = await DatabaseInitializeHelper.CreateSimpleMEntity();

            var newNEntity = new NEntity
            {
                Name = newNEntityName,
                ObjectState = EObjectState.Added
            };

            mEntity.NEntities.Add(newNEntity);
            mEntity.ObjectState = EObjectState.Modified;

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccessor.InsertOrUpdate(mEntity);
                await dataAccessor.SaveChangesAsync();
            }

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                var loadedMEntity = await dataAccessor.GetSingleAsync<MEntity>(mE => mE.Id.Equals(mEntity.Id), mE => mE.NEntities);
                Assert.AreEqual(1, loadedMEntity.NEntities.Count);
                Assert.AreEqual(newNEntityName, loadedMEntity.NEntities.First().Name);
            }
        }

        [TestMethod]
        public async Task UpdateRelatedEntityThroughParent()
        {
            const string updatedNEntityName = "updatedNEntityName";

            var mEntity = await DatabaseInitializeHelper.CreateMEntityWithSomeNEntites();
            var updatingNEntity = mEntity.NEntities.First();

            updatingNEntity.Name = updatedNEntityName;
            updatingNEntity.ObjectState = EObjectState.Modified;

            //Update through parent requires to set the parent Objectstate as "Modified"
            mEntity.ObjectState = EObjectState.Modified;

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                //Just passing the parent to the dataAccessor will do the Job
                dataAccessor.InsertOrUpdate(mEntity);
                await dataAccessor.SaveChangesAsync();
            }

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                var loadedMEntity = await dataAccessor.GetSingleAsync<MEntity>(mE => mE.Id.Equals(mEntity.Id), mE => mE.NEntities);
                Assert.AreEqual(updatedNEntityName, loadedMEntity.NEntities.Single(nE => nE.Id.Equals(updatingNEntity.Id)).Name);
            }
        }

        [TestMethod]
        public async Task UpdateRelatedEntityDirectly()
        {
            const string updatedNEntityName = "updatedNEntityName";

            var mEntity = await DatabaseInitializeHelper.CreateMEntityWithSomeNEntites();
            var updatingNEntity = mEntity.NEntities.First();

            updatingNEntity.Name = updatedNEntityName;
            updatingNEntity.ObjectState = EObjectState.Modified;

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                //Just passing the child to the dataAccessor might give us some performance improvement
                dataAccessor.InsertOrUpdate(updatingNEntity);
                await dataAccessor.SaveChangesAsync();
            }

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                var loadedMEntity = await dataAccessor.GetSingleAsync<MEntity>(mE => mE.Id.Equals(mEntity.Id), mE => mE.NEntities);
                Assert.AreEqual(updatedNEntityName, loadedMEntity.NEntities.Single(nE => nE.Id.Equals(updatingNEntity.Id)).Name);
            }
        }

        [TestMethod]
        public async Task UpdateMEntityAddExistingNEntity()
        {
            var mEntity = await DatabaseInitializeHelper.CreateSimpleMEntity();
            var nEntity = await DatabaseInitializeHelper.CreateSimpleNEntity();

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccessor.ModifyRelatedEntities(mEntity, mE => mE.NEntities, EntityState.Added, nEntity);
                await dataAccessor.SaveChangesAsync();
            }

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                var loadedMEntity = await dataAccessor.GetSingleAsync<MEntity>(mE => mE.Id.Equals(mEntity.Id), mE => mE.NEntities);
                Assert.AreEqual(nEntity.Id,loadedMEntity.NEntities.First().Id);
            }
        }

        [TestMethod]
        public async Task UpdateMEntityRemoveMultiUsedNEntity()
        {
            var firstMEntity = await DatabaseInitializeHelper.CreateMEntityWithSomeNEntites();
            var secondMEntity = await DatabaseInitializeHelper.CreateSimpleMEntity();
            var doubleUsedNEntity = firstMEntity.NEntities.First();

            //NEntity einem zweiten MEntity-Objekt hinzufügen
            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccessor.ModifyRelatedEntities(secondMEntity, mE => mE.NEntities, EntityState.Added, doubleUsedNEntity);
                await dataAccessor.SaveChangesAsync();
            }
            //Sicherstellen, dass dies funktioniert hat
            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                var reloadedSecondMEntity = await dataAccessor.GetSingleAsync<MEntity>(mE => mE.Id.Equals(secondMEntity.Id), mE => mE.NEntities);
                Assert.IsTrue(reloadedSecondMEntity.NEntities.Any(nE => nE.Id.Equals(doubleUsedNEntity.Id)));
            }
            //Entfernen des nun mehrfach verwendeten NEntity-Objekts vom ersten MEntity-Objekt
            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccessor.ModifyRelatedEntities(firstMEntity, mE => mE.NEntities, EntityState.Deleted, doubleUsedNEntity);
                await dataAccessor.SaveChangesAsync();
            }
            //Testerfolg prüfen
            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                var reloadedFirstEntity = await dataAccessor.GetSingleAsync<MEntity>(mE => mE.Id.Equals(firstMEntity.Id), mE => mE.NEntities);
                Assert.IsFalse(reloadedFirstEntity.NEntities.Any(nE => nE.Id.Equals(doubleUsedNEntity.Id)));
                var reloadedSecondMEntity = await dataAccessor.GetSingleAsync<MEntity>(mE => mE.Id.Equals(secondMEntity.Id), mE => mE.NEntities);
                Assert.IsTrue(reloadedSecondMEntity.NEntities.Any(nE => nE.Id.Equals(doubleUsedNEntity.Id)));
            }
        }

        [TestMethod]
        public async Task UpdateNEntityAddNewOtherEntity()
        {
            const string newOtherEntityName = "UpdateNEntityAddNewOtherEntity - NewOtherEntity";
            var nEntity = await DatabaseInitializeHelper.CreateSimpleNEntity();
            var newOtherEntity = new OtherEntity
            {
                Name = newOtherEntityName,
                ObjectState = EObjectState.Added,
                NEntityId = nEntity.Id,
                NEntity = nEntity
            };

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccessor.InsertOrUpdate(newOtherEntity);
                await dataAccessor.SaveChangesAsync();
            }

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                var loadedNEntity = await dataAccessor.GetSingleAsync<NEntity>(nE => nE.Id.Equals(nEntity.Id), nE => nE.OtherEntities);
                Assert.AreEqual(1, loadedNEntity.OtherEntities.Count);
                Assert.AreEqual(newOtherEntityName, loadedNEntity.OtherEntities.First().Name);
            }
        }

        [TestMethod]
        public async Task UpdateOtherEntityAttachToDifferentExistingNEntity()
        {
            var targetNEntity = await DatabaseInitializeHelper.CreateSimpleNEntity();
            var sourceNEntity = await DatabaseInitializeHelper.CreateNEntityWithSomeOtherEntities();

            var switchtingOtherEntity = sourceNEntity.OtherEntities.First();
            switchtingOtherEntity.NEntity = targetNEntity;
            switchtingOtherEntity.NEntityId = targetNEntity.Id;
            switchtingOtherEntity.ObjectState = EObjectState.Modified;


            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccessor.InsertOrUpdate(switchtingOtherEntity);
                await dataAccessor.SaveChangesAsync();
            }

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                var reloadedSourceEntity =
                    await dataAccessor.GetSingleAsync<NEntity>(nE => nE.Id.Equals(sourceNEntity.Id), nE => nE.OtherEntities);
                var reloadedTargetEntity =
                    await dataAccessor.GetSingleAsync<NEntity>(nE => nE.Id.Equals(targetNEntity.Id), nE => nE.OtherEntities);

                Assert.IsFalse(reloadedSourceEntity.OtherEntities.Any(oE => oE.Id.Equals(switchtingOtherEntity.Id)));
                Assert.AreEqual(1, reloadedSourceEntity.OtherEntities.Count);

                Assert.IsTrue(reloadedTargetEntity.OtherEntities.Any(oE => oE.Id.Equals(switchtingOtherEntity.Id)));
                Assert.AreEqual(1, reloadedTargetEntity.OtherEntities.Count);
            }

        }
    }
}
