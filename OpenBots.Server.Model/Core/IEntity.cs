using System;
using System.ComponentModel.DataAnnotations;
#nullable enable

namespace OpenBots.Server.Model.Core
{
    public interface IEntity
    {
        [Display(Name = "CreatedBy")]
        [StringLength(100,ErrorMessage ="Created by value must be 100 characters or less.")]
        string? CreatedBy { get; set; }

        [Display(Name = "CreatedOn")]
        DateTime? CreatedOn { get; set; }

        [Display(Name = "DeletedBy")]
        [StringLength(100,ErrorMessage ="Deleted by value must be 100 characters or less.")]
        string? DeletedBy { get; set; }

        [Display(Name = "DeleteOn")]
        DateTime? DeleteOn { get; set; }

        [Display(Name = "Id")]
        Guid? Id { get; set; }

        [Display(Name = "IsDeleted")]
        bool? IsDeleted { get; set; }

        [Display(Name = "Timestamp")]
        byte[] Timestamp { get; set; }

        [Display(Name = "UpdatedOn")]
        public DateTime? UpdatedOn { get; set; }

        [StringLength(100, ErrorMessage = "Deleted by value must be 100 characters or less.")]
        [Display(Name = "UpdatedBy")]
        public string? UpdatedBy { get; set; }
    }
}