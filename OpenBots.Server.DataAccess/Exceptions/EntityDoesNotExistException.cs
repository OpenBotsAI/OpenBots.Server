using System;
#nullable enable

namespace OpenBots.Server.DataAccess.Exceptions
{
    [Serializable]
    public class EntityDoesNotExistException : EntityOperationException
    {
        public EntityDoesNotExistException()
        {
        }
    }
}