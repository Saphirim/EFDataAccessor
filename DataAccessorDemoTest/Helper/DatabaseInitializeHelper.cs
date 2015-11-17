using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using DataAccessorDemo.Model.Context;
using DataAccessorDemo.Model.Entity;
using EFDataAccessor.EFDataAccessor.Accessor;
using EFDataAccessor.EFDataAccessor.Accessor.Impl;
using EFDataAccessor.EFDataAccessor.Model;

namespace DataAccessorDemoTest.Helper
{
    public static class DatabaseInitializeHelper
    {
        private static readonly IDbContextFactory<DbContext> DbContextFactory;
        static DatabaseInitializeHelper()
        {
            DbContextFactory = new DataAccessorContextFactory();
        }

        public static async Task<MEntity> CreateSimpleMEntity()
        {
            var newMEntity = new MEntity
            {
                Name = "Simple MEntity " + DateTime.Now,
                ObjectState = EObjectState.Added
            };

            using (IDataAccessor dataAccesor = new DataDataAccessor(DbContextFactory))
            {
                dataAccesor.InsertOrUpdate(newMEntity);
                await dataAccesor.SaveChangesAsync();
            }
            return newMEntity;
        }

        public static async Task<NEntity> CreateSimpleNEntity()
        {
            var newNEntity = new NEntity
            {
                Name = "Simple NEntity " + DateTime.Now,
                ObjectState = EObjectState.Added
            };

            using (IDataAccessor dataAccessor = new DataDataAccessor(DbContextFactory))
            {
                dataAccessor.InsertOrUpdate(newNEntity);
                await dataAccessor.SaveChangesAsync();
            }
            return newNEntity;
        }

        public static async Task<MEntity> CreateMEntityWithSomeNEntites()
        {
            var mEntity = new MEntity
            {
                Name = "MEntityWithSomeNEntities " + DateTime.Now,
                ObjectState = EObjectState.Added
            };

            var firstNEntity = new NEntity
            {
                Name = "FirstRelatedNEntity " + DateTime.Now,

                ObjectState = EObjectState.Added
            };

            var secondNEntitiy = new NEntity
            {
                Name = "SecondRelatedNEntity " + DateTime.Now,
                ObjectState = EObjectState.Added
            };

            mEntity.NEntities.Add(firstNEntity);
            mEntity.NEntities.Add(secondNEntitiy);

            using (IDataAccessor dataAccessor = new DataDataAccessor(DbContextFactory))
            {
                dataAccessor.InsertOrUpdate(mEntity);
                await dataAccessor.SaveChangesAsync();
            }
            return mEntity;
        }

        public static async Task<NEntity> CreateNEntityWithSomeOtherEntities()
        {
            var nEntity = new NEntity
            {
                Name = "NEntityWithOtherEntity",
                ObjectState = EObjectState.Added
            };

            var firstOtherEntity = new OtherEntity
            {
                Name = "FirstOtherEntity",
                ObjectState = EObjectState.Added
            };

            var secondOtherEntity = new OtherEntity
            {
                Name = "SecondOtherEntity",
                ObjectState = EObjectState.Added
            };

            nEntity.OtherEntities.Add(firstOtherEntity);
            nEntity.OtherEntities.Add(secondOtherEntity);

            using (IDataAccessor dataAccessor = new DataDataAccessor(DbContextFactory))
            {
                dataAccessor.InsertOrUpdate(nEntity);
                await dataAccessor.SaveChangesAsync();
            }

            return nEntity;
        }
    }
}