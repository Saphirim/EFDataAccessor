using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using DataAccessorDemo.Model.Context;
using DataAccessorDemo.Model.Entity;
using EFDataAccessor.EFDataAccessor.Accessor;
using EFDataAccessor.EFDataAccessor.Accessor.Impl;
using EFDataAccessor.EFDataAccessor.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccessorDemoTest.EFDataAccessor
{
    [TestClass]
    public class InsertTests
    {
        private IDbContextFactory<DbContext> _dbContextFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            _dbContextFactory = new DataAccessorContextFactory();
        }

        [TestMethod]
        public async Task InsertSingleEntity()
        {
            const string newEntityName = "InsertSingleEntityTest - First MEntity";
            var newMEntity = new MEntity
            {
                Name = newEntityName,
                ObjectState = EObjectState.Added
            };

            using (IDataAccessor dataAccesor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccesor.InsertOrUpdate(newMEntity);
                await dataAccesor.SaveChangesAsync();
            }

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                Assert.IsTrue(await dataAccessor.Set<MEntity>().AnyAsync(mE => mE.Name == newEntityName));
            }
        }

        [TestMethod]
        public async Task InsertMEntityWithSomeNEntities()
        {
            const string mEntityName = "InsertMEntityWithSomeNEntities - Parent MEntity";
            const string firstNEntityName = "InsertMEntityWithSomeNEntities - First NEntity";
            const string secondNEntityName = "InsertMEntityWithSomeNEntities - Second NEntity";

            var mEntity = new MEntity
            {
                Name = mEntityName,
                ObjectState = EObjectState.Added
            };

            var firstNEntity = new NEntity
            {
                Name = firstNEntityName,
                ObjectState = EObjectState.Added
            };

            var secondNEntitiy = new NEntity
            {
                Name = secondNEntityName,
                ObjectState = EObjectState.Added
            };

            mEntity.NEntities.Add(firstNEntity);
            mEntity.NEntities.Add(secondNEntitiy);

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccessor.InsertOrUpdate(mEntity);
                await dataAccessor.SaveChangesAsync();
            }

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                Assert.IsTrue(await dataAccessor.Set<MEntity>().AnyAsync(mE => mE.Name.EndsWith(mEntityName)));
                var loadedMEntity = await dataAccessor.GetSingleAsync<MEntity>(entity => entity.Name.EndsWith(mEntityName),
                    entity => entity.NEntities);
                Assert.AreEqual(2, loadedMEntity.NEntities.Count);
                Assert.IsTrue(loadedMEntity.NEntities.Any(nE => nE.Name.Equals(secondNEntityName)));
                Assert.IsTrue(loadedMEntity.NEntities.Any(nE => nE.Name.Equals(firstNEntityName)));
            }
        }

        [TestMethod]
        public async Task InsertNEntityWithSomeOtherEntities()
        {
            const string nEntityName = "InsertNEntityWithOtherEntities - NEntity";
            const string firstOtherEntityName = "InsertNEntityWithOtherEntities - FirstOtherEntity";
            const string secondOtherEntityName = "InsertNEntityWithOtherEntities - SecondOtherEntity";



            var nEntity = new NEntity
            {
                Name = nEntityName,
                ObjectState = EObjectState.Added
            };

            var firstOtherEntity = new OtherEntity
            {
                Name = firstOtherEntityName,
                ObjectState = EObjectState.Added
            };

            var secondOtherEntity = new OtherEntity
            {
                Name = secondOtherEntityName,
                ObjectState = EObjectState.Added
            };

            nEntity.OtherEntities.Add(firstOtherEntity);
            nEntity.OtherEntities.Add(secondOtherEntity);

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccessor.InsertOrUpdate(nEntity);
                await dataAccessor.SaveChangesAsync();
            }

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                Assert.IsTrue(await dataAccessor.Set<NEntity>().AnyAsync(nE => nE.Name.Equals(nEntityName)));
                var loadedNEntity = await dataAccessor.GetSingleAsync<NEntity>(nE => nE.Name.Equals(nEntityName), entity => entity.OtherEntities);
                Assert.AreEqual(2, loadedNEntity.OtherEntities.Count);
                Assert.IsTrue(loadedNEntity.OtherEntities.Any(oE => oE.Name.Equals(firstOtherEntityName)));
                Assert.IsTrue(loadedNEntity.OtherEntities.Any(oE => oE.Name.Equals(secondOtherEntityName)));
            }

        }

        [TestMethod]
        public async Task InsertMEntityWithSomeNEntitiesWithSomeOtherEntities()
        {
            const string mEntitiyName = "InsertMEntityWithSomeNEntitiesWithSomeOtherEntities - MEntity";

            const string firstNEntityName = "InsertMEntityWithSomeNEntitiesWithSomeOtherEntities - firstNEntity";
            const string secondNEntityName = "InsertMEntityWithSomeNEntitiesWithSomeOtherEntities - secondNEntity";

            const string fNEfirstOtherEntityName =
                "InsertMEntityWithSomeNEntitiesWithSomeOtherEntities - fNEfirstOtherEntityName";
            const string fNEsecondOtherEntityName =
                "InsertMEntityWithSomeNEntitiesWithSomeOtherEntities - fNEsecondOtherEntityName";
            const string sNefirstOtherEntityName =
                "InsertMEntityWithSomeNEntitiesWithSomeOtherEntities - sNEfirstOtherEntityName";

            var mEntity = new MEntity
            {
                Name = mEntitiyName,
                ObjectState = EObjectState.Added,
                NEntities = new ObservableCollection<NEntity>
                {
                    new NEntity
                    {
                        Name = firstNEntityName,
                        ObjectState = EObjectState.Added,
                        OtherEntities = new ObservableCollection<OtherEntity>
                        {
                            new OtherEntity
                            {
                                Name = fNEfirstOtherEntityName,
                                ObjectState = EObjectState.Added
                            },
                            new OtherEntity
                            {
                                Name = fNEsecondOtherEntityName,
                                ObjectState = EObjectState.Added
                            }
                        }
                    },
                        new NEntity
                        {
                            Name = secondNEntityName,
                            ObjectState = EObjectState.Added,
                            OtherEntities = new ObservableCollection<OtherEntity>
                            {
                                new OtherEntity
                                {
                                    Name = sNefirstOtherEntityName,
                                    ObjectState = EObjectState.Added
                                }
                            }
                        }
                }
            };

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccessor.InsertOrUpdate(mEntity);
                await dataAccessor.SaveChangesAsync();
            }

            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                Assert.IsTrue(await dataAccessor.Set<MEntity>().AnyAsync(mE => mE.Name.Equals(mEntitiyName)));
                var loadedMEntity = await dataAccessor.GetSingleAsync<MEntity>(mE => mE.Name.Equals(mEntitiyName),
                    mE => mE.NEntities,
                    mE => mE.NEntities.Select(nE => nE.OtherEntities));

                Assert.AreEqual(2, loadedMEntity.NEntities.Count);
                Assert.AreEqual(2, loadedMEntity.NEntities.Single(nE => nE.Name.Equals(firstNEntityName)).OtherEntities.Count);
            }
        }

        [TestMethod, ExpectedException(typeof(ValidationException))]
        public async Task TrySaveInvalidEntity()
        {
            var otherEntity = new OtherEntity
            {
                Name = "InsertMEntityWithSomeNEntitiesWithSomeOtherEntities - otherEntity without navigation property",
                ObjectState = EObjectState.Added
            };
            using (IDataAccessor dataAccessor = new DataDataAccessor(_dbContextFactory))
            {
                dataAccessor.InsertOrUpdate(otherEntity);
                Assert.Fail("Bei InsertOrUpdate sollte eine Validationexception geflogen sein!");
                await dataAccessor.SaveChangesAsync();
            }
        }
    }
}
