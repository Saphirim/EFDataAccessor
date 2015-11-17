using System.Data.Entity;
using EFDataAccessor.EFDataAccessor.Model;

namespace EFDataAccessor.EFDataAccessor.Accessor.Impl
{
    public class HelperFunctions
    {
        public static EntityState ConvertState(EObjectState state)
        {
            switch (state)
            {
                case EObjectState.Added:
                    return EntityState.Added;
                case EObjectState.Modified:
                    return EntityState.Modified;
                case EObjectState.Deleted:
                    return EntityState.Deleted;
                case EObjectState.Processed:
                    return EntityState.Unchanged;
                default:
                    return EntityState.Unchanged;
            }
        }
    }
}