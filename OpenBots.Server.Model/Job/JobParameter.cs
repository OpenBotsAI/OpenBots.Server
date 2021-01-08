using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model
{
    /// <summary>
    /// Stores the values corresponding to a job's parameters
    /// </summary>
    public class JobParameter : NamedEntity
    {
        [Required]
        [Display(Name = "DataType")]
        public string DataType { get; set; }

        [Required]
        [Display(Name = "Value")]
        public string Value { get; set; }

        [Display(Name = "JobId")]
        public Guid JobId { get; set; }
    }
}
