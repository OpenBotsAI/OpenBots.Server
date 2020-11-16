using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenBots.Server.Model
{
    public class JobParameter : NamedEntity
    {
        [Required]
        [Display(Name = "DataType")]
        public string DataType { get; set; }

        [Required]
        [Display(Name = "Value")]
        public string Value { get; set; }

        [Required]
        [Display(Name = "JobId")]
        public Guid? JobId { get; set; }
    }
}
