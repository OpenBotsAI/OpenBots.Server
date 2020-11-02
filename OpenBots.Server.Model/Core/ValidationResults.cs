using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
#nullable enable

namespace OpenBots.Server.Model.Core
{
    public class ValidationResults : List<ValidationResult>
    {
        public ValidationResults()
        {
        }

        [Display(Name = "IsValid")]
        public bool IsValid { get; set; }
    }
}