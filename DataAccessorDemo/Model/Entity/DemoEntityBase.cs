using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EFDataAccessor.EFDataAccessor.Model;

namespace DataAccessorDemo.Model.Entity
{
    public class DemoDataAccessorEntityBase : IObjectStateEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual Guid Id { get; set; }

        [NotMapped]
        public EObjectState ObjectState { get; set; }
    }
}