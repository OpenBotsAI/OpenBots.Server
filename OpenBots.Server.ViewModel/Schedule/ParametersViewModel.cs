using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

        public static void VerifyParameterNameAvailability(IEnumerable<ParametersViewModel> genericParameters)
        {
            var set = new HashSet<string>();

            foreach (var parameter in genericParameters ?? Enumerable.Empty<ParametersViewModel>())
            {
                if (!set.Add(parameter.Name))
                {
                    throw new Exception($"Parameter name \"{parameter.Name}\", is a duplicate");
                }
            }
        }
    }
}
