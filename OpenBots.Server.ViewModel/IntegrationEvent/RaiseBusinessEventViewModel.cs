using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.ViewModel
{
    public class RaiseBusinessEventViewModel
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string Message { get; set; }
        public string PayloadJSON { get; set;  }

    }
}
