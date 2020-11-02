using System;

namespace OpenBots.Server.Infrastructure.Email
{
    public class CannotSendEmailException : SendEmailException
    {
        public CannotSendEmailException()
        {
        }

        public CannotSendEmailException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
