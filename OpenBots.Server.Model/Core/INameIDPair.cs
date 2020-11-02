using System;
using System.ComponentModel.DataAnnotations;
#nullable enable

namespace OpenBots.Server.Model.Core
{
    public interface INameIDPair
    {
        [Display(Name = "Id")]
        public Guid? Id { get; set; }

        [Display(Name = "Name")]
        string Name { get; set; }
    }
}
