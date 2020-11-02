using System;

namespace OpenBots.Server.Model
{
    /// <summary>
    /// Attribute to determine if a field will not be shown in audit log
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DoNotAuditAttribute : Attribute
    {
        /// <summary>
        /// NonAuditable Attribute constructor
        /// </summary>
        /// <param name="nonauditable"></param>
        public DoNotAuditAttribute(bool nonauditable = true)
        {
            Nonauditable = nonauditable;
        }

        /// <summary>
        /// Property to get nonauditable boolean value
        /// </summary>
        public bool Nonauditable { get; }
    }
}