using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using DataAccessorDemo.Model.Entity;

namespace DataAccessorDemo.Model.Context
{
    [ExcludeFromCodeCoverage]
    public class DataAccessorContext : DbContext
    {
        public DataAccessorContext() : base("DataAccessorDemo")
        {
            //Für das Demoprojekt soll die Datenbank stets neu erstellt werden.
            Database.SetInitializer(new DropCreateDatabaseAlways<DataAccessorContext>());
        }

        #region DbSets

        public virtual DbSet<MEntity> MEntities { get; set; }
        public virtual DbSet<NEntity> NEntities { get; set; }
        public virtual DbSet<OtherEntity> OtherEntities { get; set; }

        #endregion
    }
}