using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class CreateIPFencingViewModel : IViewModel<CreateIPFencingViewModel, IPFencing>
    {
        [Required]
        [Display(Name = "Usage")]
        public UsageType? Usage { get; set; }

        [Required]
        [Display(Name = "Rule")]
        public RuleType? Rule { get; set; }

        [Display(Name = "IPAddress")]
        public string? IPAddress { get; set; }

        [Display(Name = "IPRange")]
        public string? IPRange { get; set; }

        [Display(Name = "HeaderName")]
        public string? HeaderName { get; set; }

        [Display(Name = "HeaderValue")]
        public string? HeaderValue { get; set; }

        public IPFencing Map(CreateIPFencingViewModel viewModel)
        {
            IPFencing iPFencing = new IPFencing
            {
                Usage = viewModel.Usage,
                Rule = viewModel.Rule,
                IPAddress = viewModel.IPAddress,
                IPRange = viewModel.IPRange,
                HeaderName = viewModel.HeaderName,
                HeaderValue = viewModel.HeaderValue
            };

            return iPFencing;
        }
    }
}
