using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model
{
    /// <summary>
    /// Stores the values corresponding to a schedule's parameters
    /// </summary>
    public class ScheduleParameter : NamedEntity
    {
        [Required]
        [Display(Name = "DataType")]
        public string DataType { get; set; }

        [Required]
        [Display(Name = "Value")]
        public string Value { get; set; }

        [Display(Name = "ScheduleId")]
        public Guid ScheduleId { get; set; }
    }
}
