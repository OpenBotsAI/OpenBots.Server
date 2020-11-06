using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.ViewModel.Email
{
    public class EmailTemplateData
    {
        public string Password { get; set; }
        public string HrefLink { get; set; }
        public string Url {get; set;}
        public string ApiUrl { get; set; }
        public string FileName { get; set; }
    }
}
