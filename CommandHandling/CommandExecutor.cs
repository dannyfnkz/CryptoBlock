using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace CommandHandling
    {
        public abstract class CommandExecutor
        {
            public class InvalidCommandSyntaxException : CommandExecutionException
            {
                public InvalidCommandSyntaxException(string commandType)
                    : base(formatExceptionMessage(commandType))
                {

                }

                private static string formatExceptionMessage(string commandType)
                {
                    return string.Format("Invalid syntax for command type {0}.", commandType);
                }
            }

            public abstract bool IsValidCommand(string userInputLowercase);
            public abstract string GetCommandPrefix(string userInputLowercase);
            public abstract void ExecuteCommand(string commandPrefix, string[] commandArguments);
        }
    }
}