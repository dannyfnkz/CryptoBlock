using System;
using CryptoBlock.CommandHandling;
using CryptoBlock.IOManagement;
using CryptoBlock.PortfolioManagement;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.SystemManagement;
using CryptoBlock.Utils;

namespace CryptoBlock
{
    public static class CommandParser
    {

        public class CommandParseException : Exception
        {
            public CommandParseException(string message) : base(message)
            {

            }
        }

        public class InvalidCommandException : CommandParseException
        {
            public InvalidCommandException() : base("Invalid command.")
            {

            }
        }

        public class WrongNumberOfArgumentsException : CommandParseException
        {
            public WrongNumberOfArgumentsException() : base("Wrong number of arguments.")
            {

            }
        }

        private static readonly CommandExecutor[] commandExecutors = new CommandExecutor[]
        {
                new ServerDataCommandExecutor(), 
                new PortfolioCommandExecutor(),
                new SystemCommandExecutor()
        };

        public static void ParseCommand(string userInput)
        {
            // commands are case insensitive
            string userInputLowercase = userInput.ToLower();

            // look for a command executor which recognizes user input as a command
            foreach (CommandExecutor commandExecutor in commandExecutors)
            {
                if (commandExecutor.IsValidCommand(userInputLowercase)) // command recognized
                {
                    string prefix = commandExecutor.GetCommandPrefix(userInputLowercase);

                    // if there's a space directly after prefix, split by prefix + space
                    // else split by prefix
                    string splitPrefix = userInputLowercase.Contains(prefix + " ") ? prefix + " " : prefix;
                    string commandArgumentString = StringUtils.Substring(
                        userInputLowercase,
                        splitPrefix);

                    // get command arguments array
                    string[] commandArguments;

                    if(commandArgumentString == string.Empty) // empty command args string
                    {
                        commandArguments = new string[0];
                    }
                    else // non-empty command args string
                    {
                        commandArguments = StringUtils.Split(commandArgumentString, " ");
                    }

                    commandExecutor.ExecuteCommand(prefix, commandArguments);

                    return;
                }         
            }

            // user input not recognized by any command executor
            logUnrecognizedCommandExecption(userInput);
        }

        private static void logUnrecognizedCommandExecption(string userInput)
        {
            ConsoleIOManager.Instance.LogErrorFormat(
                false,
                "Unrecognized command: '{0}'.",             
                userInput);
        }
    }
}
