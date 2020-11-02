using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.Model.Options
{
    public class WebAppUrlOptions
    {
        public const string WebAppUrl = "WebAppUrl";

        public string Url {get; set;}
        public string login { get; set; }
        public string forgotpassword { get; set; }
        public string tokenerror { get; set; }
        public string NoUserExists { get; set; }
        public string emailaddressconfirmed { get; set; }
    }
}
