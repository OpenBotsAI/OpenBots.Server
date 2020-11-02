using Microsoft.AspNetCore.Mvc;
using System;

namespace OpenBots.Server.Model.Attributes
{
    /// <summary>
    /// Attribute for API version 1.0
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class V1Attribute : ApiVersionAttribute
    {
        /// <summary>
        /// Version 1 Attribute
        /// </summary>
        public V1Attribute() : base(new ApiVersion(1, 0)) { }
    }
}
