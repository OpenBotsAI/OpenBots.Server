using OpenBots.Server.Model;

namespace OpenBots.Server.ViewModel
{
    public class NextJobViewModel
    {
        public bool IsJobAvailable { get; set; }
        public Job AssignedJob { get; set; }
    }
}
