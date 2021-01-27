﻿using System;
#nullable enable

namespace OpenBots.Server.DataAccess.Exceptions
{
    [Serializable]
    public class EntityAlreadyExistsException : EntityOperationException
    {
        public EntityAlreadyExistsException()
        {
        }

        public EntityAlreadyExistsException(string? message) : base(message)
        {
        }
    }
}