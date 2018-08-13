using System;

namespace CryptoBlock
{
    namespace CommandHandling.Arguments
    {
        public class CommandArgumentParseException : CommandExecutionException
        {
            public CommandArgumentParseException(
                string commandPrefix,
                string additionalInfo = null,
                Exception innerException = null)
                : base(formatExceptionMessage(commandPrefix, additionalInfo), innerException)
            {

            }

            private static string formatExceptionMessage(
                string commandPrefix, 
                string additionalInfo)
            {
                string messagePostfix = additionalInfo == null
                    ? "."
                    : string.Format(": {0}.", additionalInfo);

                string exceptionMessage = string.Format(
                    "Could not parse '{0}' command argument(s){1}",
                    commandPrefix,
                    messagePostfix);

                return exceptionMessage;
            }
        }
    }
}