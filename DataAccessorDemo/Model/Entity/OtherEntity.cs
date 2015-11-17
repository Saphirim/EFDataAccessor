using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessorDemo.Model.Entity
{
    [Table("OtherEntities")]
    public class OtherEntity : DemoDataAccessorEntityBase
    {
        public string Name { get; set; }

        public Guid NEntityId { get; set; }
        private NEntity _nEntity;
        [Required, ForeignKey(nameof(NEntityId))]
        public virtual NEntity NEntity
        {
            get { return _nEntity; }
            set { _nEntity = value; }
        }
    }
}