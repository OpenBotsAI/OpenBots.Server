using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class QueueViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        public int MaxRetryCount { get; set; }
    }
}
