using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DataAccessorDemo.Model.Entity
{
    [Table("NEntities")]
    public class NEntity : DemoDataAccessorEntityBase
    {
        public string Name { get; set; }

        private ObservableCollection<OtherEntity> _otherEntities;
        public virtual ObservableCollection<OtherEntity> OtherEntities
        {
            get { return _otherEntities ?? (_otherEntities = new ObservableCollection<OtherEntity>()); }
            [ExcludeFromCodeCoverage]
            set { _otherEntities = value; }
        }

        private ObservableCollection<MEntity> _mEntities;
        public virtual ObservableCollection<MEntity> MEntities
        {
            get { return _mEntities ?? (_mEntities = new ObservableCollection<MEntity>()); }
            [ExcludeFromCodeCoverage]
            set { _mEntities = value; }
        }
    }
}