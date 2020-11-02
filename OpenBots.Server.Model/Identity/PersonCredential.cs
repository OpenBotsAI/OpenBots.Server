using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable enable

namespace OpenBots.Server.Model.Identity
{
    public class PersonCredential : Entity
    {
        [Required]
        [Display(Name = "PersonId")]
        public Guid? PersonId { get; set; }

        [Display(Name = "Secret")]
        public string? Secret { get; set; }

        [Display(Name = "Salt")]
        public string? Salt { get; set; }

        [DefaultValue(false)]
        [Display(Name = "IsExpired")]
        public bool? IsExpired { get; set; }

        [Display(Name = "ExpiresOnUTC")]
        public DateTime? ExpiresOnUTC { get; set; }

        [Display(Name = "ForceChange")]
        [DefaultValue(false)]
        public bool? ForceChange { get; set; }

        [ForeignKey("PersonId")]
        [Display(Name ="Person")]
        public Person? Person { get; set; }
    }
}
