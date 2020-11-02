using System;
using OpenBots.Server.Model.Core;
#nullable enable

namespace OpenBots.Server.DataAccess.Exceptions
{
    [Serializable]
    public class UnauthorizedOperationException : EntityOperationException
    {
        public EntityOperationType Operation { get; private set; }

        public UnauthorizedOperationException()
        {
            Operation = EntityOperationType.Unknown;
        }

        public UnauthorizedOperationException(EntityOperationType add)
        {
            Operation = add;
        }
    }
}