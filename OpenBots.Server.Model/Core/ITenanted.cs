using System;
using System.ComponentModel.DataAnnotations;
#nullable enable

namespace OpenBots.Server.Model.Core
{
    public interface ITenanted
    {
        [Required]
        [Display(Name = "OrganizationId")]
        public Guid? OrganizationId { get; set;}
    }
}