using System;
using System.Runtime.Serialization;

namespace OpenBots.Server.Infrastructure.Email
{
    public class SendEmailException : Exception
    {
        public SendEmailException()
        {
        }

        public SendEmailException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
