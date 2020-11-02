using System.ComponentModel.DataAnnotations;
#nullable enable

namespace OpenBots.Server.Model.Core
{
    public abstract class NamedEntity : Entity, INamedEntity
    {
        public NamedEntity() : base()
        {
        }

        [MaxLength(100,ErrorMessage ="Name must be 100 characters or less.")]
        [Required]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters.")]
        [RegularExpression("^[A-Za-z0-9_.-]{3,100}$")] // Alphanumeric with Underscore, Hyphen and Dot only
        [Display(Name= "Name")]
        public string? Name { get; set; }
    }
}
