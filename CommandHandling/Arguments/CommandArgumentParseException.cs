using System;

namespace CryptoBlock
{
    namespace CommandHandling.Arguments
    {
        /// <summary>
        /// thrown if a command argument could not be parsed from a corresponding element
        /// in (string) command argument array passed to <see cref="Command"/> on
        /// <see cref="Command.Execute(string[])"/>.
        /// </summary>
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