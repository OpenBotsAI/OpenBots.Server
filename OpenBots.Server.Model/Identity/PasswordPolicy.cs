using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
#nullable enable

namespace OpenBots.Server.Model.Identity
{
    public class PasswordPolicy : Entity
    {
        [Display(Name = "MinimumLength")]
        public int? MinimumLength { get; set; }

        [DefaultValue(false)]
        [Display(Name = "RequireAtleastOneUppercase")]
        public bool? RequireAtleastOneUppercase { get; set; }

        [DefaultValue(false)]
        [Display(Name = "RequireAtleastOneLowercase")]
        public bool? RequireAtleastOneLowercase { get; set; }

        [DefaultValue(false)]
        [Display(Name = "RequireAtleastOneNonAlpha")]
        public bool? RequireAtleastOneNonAlpha { get; set; }

        [DefaultValue(false)]
        [Display(Name = "RequireAtleastOneNumber")]
        public bool? RequireAtleastOneNumber { get; set; }

        [DefaultValue(false)]
        [Display(Name = "EnableExpiration")]
        public bool? EnableExpiration { get; set; }

        [Range(1, 365,ErrorMessage ="The range for expiration must be between 1 and 365.")]
        [Display(Name = "ExpiresInDays")]
        public int? ExpiresInDays { get; set; }
    }
}
