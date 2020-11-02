using OpenBots.Server.Model.Identity;
using System;
using System.ComponentModel.DataAnnotations;
#nullable enable

namespace OpenBots.Server.Model.Core
{
    public enum EntityOperationType : int
    {
        Unknown = 0,
        Add = 1,
        Update = 2,
        Delete = 3,
        HardDelete = 4,
        Other =9
    }
}
