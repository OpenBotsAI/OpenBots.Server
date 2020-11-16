using System;
#nullable enable

namespace OpenBots.Server.DataAccess.Exceptions
{
    [Serializable]
    public class EntityConcurrencyException : EntityOperationException
    {
        public EntityConcurrencyException()
        {
        }

        public EntityConcurrencyException(Exception exception) : base(exception)
        {

        }
    }
}