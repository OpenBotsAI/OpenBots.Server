using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.Core
{
    public class ApplicationVersion : NamedEntity
    {
        [Required]
        [Display(Name = "Major")]
        public int Major { get; set; }

        [Required]
        [Display(Name = "Minor")]
        public int? Minor { get; set; }

        [Required]
        [Display(Name = "Patch")]
        public int? Patch { get; set; }
    }
}
