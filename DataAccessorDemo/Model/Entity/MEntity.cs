using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DataAccessorDemo.Model.Entity
{
    [Table("MEntities")]
    public class MEntity : DemoDataAccessorEntityBase
    {
        public string Name { get; set; }

        private ObservableCollection<NEntity> _nEntities;
        public virtual ObservableCollection<NEntity> NEntities
        {
            get { return _nEntities ?? (_nEntities = new ObservableCollection<NEntity>()); }
            [ExcludeFromCodeCoverage]
            set { _nEntities = value; }
        }
    }
}