using System;
using System.Runtime.Serialization;
#nullable enable

namespace OpenBots.Server.DataAccess.Exceptions
{
    [Serializable]
    public class EntityOperationException : Exception
    {
        public EntityOperationException()
        {
        }

        public EntityOperationException(string? message) : base(message)
        {
        }

        public EntityOperationException(Exception innerException) : base(innerException?.Message, innerException)
        {

        }

        public EntityOperationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EntityOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}