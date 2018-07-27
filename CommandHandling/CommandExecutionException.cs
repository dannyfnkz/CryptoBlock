using System;

namespace CryptoBlock
{
    namespace CommandHandling
    {
        /// <summary>
        /// thrown when an exception occurs during the execution of a command.
        /// </summary>
        public class CommandExecutionException : Exception
        {
            public CommandExecutionException(string message, Exception innerException)
                 : base(message, innerException)
            {

            }

            public CommandExecutionException(string message)
                : base(message)
            {

            }

            public CommandExecutionException(Exception innerException)
                 : base(string.Empty, innerException)
            {

            }
        }
    }
}