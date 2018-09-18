using System;
using System.Collections.Generic;
using CryptoBlock.CommandHandling;
using CryptoBlock.CommandHandling.Arguments;
using CryptoBlock.ConfigurationManagement;
using CryptoBlock.IOManagement;
using CryptoBlock.PortfolioManagement;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.SystemManagement;
using CryptoBlock.Utils;
using CryptoBlock.Utils.Strings;

namespace CryptoBlock
{
    public class CommandParsingManager
    {
        private class CommandParsingManagerCommandExecutor : CommandExecutor
        {
            private const string COMMAND_TYPE = "CommandParser";

            public CommandParsingManagerCommandExecutor()
            {
                // add associations between commands and their prefixes
 
            }

            /// <summary>
            /// returns <see cref="ServerDataCommandExecutor"/> command type.
            /// </summary>
            public override string CommandType
            {
                get { return COMMAND_TYPE; }
            }
        }

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

        private const string COMMAND_ARGUMENT_DELIMITER = @" ";
        private const string COMMAND_ARGUMENT_WRAPPER = @"'";

        private static CommandParsingManager instance;

        private CommandParsingManager()
        {
            
        }

        public static void Initialize()
        {
            instance = new CommandParsingManager();
        }

        public static CommandParsingManager Instance
        {
            get { return instance; }
        }

        public void ParseCommand(string userInput)
        {
            // commands are case insensitive
            string userInputLowercase = userInput.ToLower();

            string commandString;

            // user input represents a UserDefinedCommand alias 
            if(ConfigurationManager.Instance.UserDefinedCommandExists(userInputLowercase))
            {
                commandString = ConfigurationManager.Instance
                    .GetUserDefinedCommand(userInputLowercase).CommandString;
            }
            else // user input does not represent a UserDefinedCommand alias 
            {
                commandString = userInputLowercase;
            }

            // look for a command executor which recognizes user input as a command
            foreach (CommandExecutor commandExecutor in commandExecutors)
            {
                if (commandExecutor.IsValidCommand(commandString)) // command recognized
                {
                    string prefix = commandExecutor.GetCommandPrefix(commandString);

                    // if there's a space directly after prefix, split by prefix + space
                    // else split by prefix
                    string splitPrefix = commandString.Contains(prefix + " ") ? prefix + " " : prefix;
                    string commandArgumentString =
                        commandString.GetSubstringAfterPrefix(splitPrefix);

                    // get command arguments array
                    string[] commandArguments;

                    if(commandArgumentString == string.Empty) // empty command args string
                    {
                        commandArguments = new string[0];
                    }
                    else // non-empty command args string
                    {
                        // extract command arguments from commandArgumentString
                        commandArguments = commandArgumentString.SplitByBlocks(
                            COMMAND_ARGUMENT_DELIMITER,
                            COMMAND_ARGUMENT_WRAPPER,
                            COMMAND_ARGUMENT_WRAPPER);
                    }

                    // handle command
                    commandExecutor.HandleCommand(prefix, commandArguments);

                    return;
                }         
            }

            // user input not recognized by any command executor
            logUnrecognizedCommandExecption(commandString);
        }

        private void logUnrecognizedCommandExecption(string commandString)
        {
            ConsoleIOManager.Instance.LogErrorFormat(
                false,
                ConsoleIOManager.eOutputReportType.CommandExecution,
                "Unrecognized command: '{0}'.",
                commandString);
        }
    }
}
