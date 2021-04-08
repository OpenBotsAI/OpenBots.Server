using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.Model
{
    public class TimeZoneId : Entity
    {
        [Display(Name = "WindowsTimeZone")]
        public string WindowsTimeZone { get; set; }

        [Display(Name = "LinuxTimeZone")]
        public string LinuxTimeZone { get; set; }
    }
}
