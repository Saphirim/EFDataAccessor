using EFDataAccessor.EFDataAccessor.Accessor;

namespace EFDataAccessor.EFDataAccessor.Factory
{
    public interface IDataAccessorFactory
    {
        IDataAccessor Create();
    }
}