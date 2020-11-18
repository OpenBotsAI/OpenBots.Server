using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.Model
{
    /// <summary>
    /// Stores the values corresponding to a job's checkpoints
    /// </summary>
    public class JobCheckpoint : NamedEntity, INonAuditable
    {
        [Display(Name = "Message")]
        public string Message { get; set; }

        [Display(Name = "Iterator")]
        public string Iterator { get; set; }

        [Display(Name = "IteratorValue")]
        public string IteratorValue { get; set; }

        [Display(Name = "IteratorPosition")]
        public int? IteratorPosition { get; set; }

        [Display(Name = "IteratorCount")]
        public int? IteratorCount { get; set; }

        [Display(Name = "JobId")]
        public Guid JobId { get; set; }
    }
}
