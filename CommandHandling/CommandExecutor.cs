using CryptoBlock.Utils;
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

            private readonly Dictionary<string, Command> commandPrefixToCommmand
                = new Dictionary<string, Command>();

            protected abstract string GetCommandType();

            public bool IsValidCommand(string userInputLowercase)
            {
                // check if user input starts with a recognized command prefix
                foreach (string commandPrefix in commandPrefixToCommmand.Keys)
                {
                    if (userInputLowercase.StartsWith(commandPrefix))
                    {
                        return true;
                    }
                }

                return false;
            }

            public string GetCommandPrefix(string userInputLowercase)
            {
                // if user input starts with a recognized prefix, get prefix
                string prefix = StringUtils.GetPrefixIfStartsWith(
                    userInputLowercase,
                    commandPrefixToCommmand.Keys.ToArray());

                if (prefix != null) // valid command
                {
                    return prefix;
                }
                else // invalid command
                {
                    throw new InvalidCommandSyntaxException(GetCommandType());
                }
            }

            public void ExecuteCommand(string commandPrefix, string[] commandArguments)
            {
                if (!IsValidCommand(commandPrefix))
                {
                    throw new InvalidCommandSyntaxException(GetCommandType());
                }

                // get matching command
                Command command = commandPrefixToCommmand[commandPrefix];

                command.ExecuteCommand(commandArguments);
            }

            protected void AddPrefixToCommandPair(string commandPrefix, Command command)
            {
                commandPrefixToCommmand.Add(commandPrefix, command);
            }

            protected void AddPrefixToCommandPair(params Command[] commands)
            {
                foreach(Command command in commands)
                {
                    string portfolioEntryCommandPrefix = command.Prefix;
                    commandPrefixToCommmand[portfolioEntryCommandPrefix] = command;
                }
            }

            protected void AddPrefixToCommandPair(KeyValuePair<string, Command> prefixCommandPair)
            {
                AddPrefixToCommandPair(prefixCommandPair.Key, prefixCommandPair.Value);
            }

            protected void AddPrefixToCommandPairRange(
                IEnumerable<KeyValuePair<string, Command>> prefixCommandPairs)
            {
                foreach (KeyValuePair<string, Command> prefixCommandPair in prefixCommandPairs)
                {
                    AddPrefixToCommandPair(prefixCommandPair.Key, prefixCommandPair.Value);
                }
            }

            protected Command GetCommand(string commandPrefix)
            {
                return commandPrefixToCommmand[commandPrefix];
            }
        }
    }
}