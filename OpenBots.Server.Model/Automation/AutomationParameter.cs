using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.Model
{
    public class AutomationParameter : NamedEntity
    {
        [Required]
        [Display(Name = "DataType")]
        public string DataType { get; set; }

        [Display(Name = "Value")]
        public string Value { get; set; }

        [Display(Name = "AutomationId")]
        public Guid AutomationId { get; set; }
    }
}
