using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
#nullable enable

namespace OpenBots.Server.Model.Core
{
    public abstract class Entity : IEntity
    {
        public Entity()
        {
            Id = Guid.NewGuid();
            Timestamp = new byte[1];
            CreatedBy = "";
            DeletedBy = "";
            IsDeleted = false;
        }

        [Key]
        [Display(Name = "Id")]
        public Guid? Id { get; set; }

        [DefaultValue(false)]
        [Display(Name = "IsDeleted")]
        public bool? IsDeleted { get; set; }

        [MaxLength(100,ErrorMessage ="Created by value must be 100 characters or less.")]
        [Display(Name = "CreatedBy")]
        public string? CreatedBy { get; set; }

        [Display(Name = "CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [MaxLength(100,ErrorMessage ="Deleted by value must be 100 characters or less.")]
        [Display(Name = "DeletedBy")]
        public string? DeletedBy { get; set; }

        [Display(Name = "DeleteOn")]
        public DateTime? DeleteOn { get; set; }

        [Timestamp]
        [Display(Name ="Timestamp")]
        public byte[] Timestamp { get; set; }

        [Display(Name = "UpdatedOn")]
        public DateTime? UpdatedOn { get; set; }

        [StringLength(100, ErrorMessage = "Deleted by value must be 100 characters or less.")]
        [Display(Name = "UpdatedBy")]
        public string? UpdatedBy { get; set; }
    }
}
