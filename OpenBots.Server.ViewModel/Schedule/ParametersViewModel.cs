using OpenBots.Server.Model.Core;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    /// <summary>
    /// Used to pass parameters when creating or executing schedules
    /// </summary>
    public class ParametersViewModel : NamedEntity
    {
        [Required]
        [Display(Name = "DataType")]
        public string DataType { get; set; }

        [Required]
        [Display(Name = "Value")]
        public string Value { get; set; }
    }
}
