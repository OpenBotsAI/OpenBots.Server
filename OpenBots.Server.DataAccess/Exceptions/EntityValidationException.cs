using System;
using OpenBots.Server.Model.Core;
#nullable enable

namespace OpenBots.Server.DataAccess.Exceptions
{
    [Serializable]
    public class EntityValidationException : EntityOperationException
    {
        public ValidationResults Validation { get; private set; }

        public EntityValidationException()
        {
            Validation = new ValidationResults();
        }

        public EntityValidationException(ValidationResults validation)
        {
            Validation = validation;
        }     
    }
}