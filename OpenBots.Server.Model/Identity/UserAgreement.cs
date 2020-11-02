using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.Identity
{
    public class UserAgreement : Entity
    {
        [Required]
        [Display(Name = "Version")]
        public int? Version { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "ContentStaticUrl")]
        public string? ContentStaticUrl { get; set; }

        [Display(Name = "EffectiveOnUTC")]
        public DateTime? EffectiveOnUTC { get; set; }

        [Display(Name = "ExpiresOnUTC")]
        public DateTime? ExpiresOnUTC { get; set; }
    }
}
