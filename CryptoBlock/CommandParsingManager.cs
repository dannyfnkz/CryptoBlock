using System;
using CryptoBlock.CommandHandling;
using CryptoBlock.CommandHandling.Arguments;
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
            private class CommandReverseCommand : Command
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 0;
                private const int MAX_NUMBER_OF_ARGUMENTS = 0;

                private const string PREFIX = "undo";

                internal CommandReverseCommand()
                    : base(PREFIX)
                {
                    base.commandArgumentConstraintList.Add(
                        new NumberOfArgumentsCommandArgumentConstraint(
                            MIN_NUMBER_OF_ARGUMENTS,
                            MAX_NUMBER_OF_ARGUMENTS)
                        );
                }

                protected override bool Execute(string[] commandArguments)
                {
                    bool commandExecutedSuccessfuly;

                    Command lastExecutedCommand = CommandParsingManager.instance.lastExecutedCommand;

                    if (lastExecutedCommand != null)
                    {
                        if(lastExecutedCommand.Executed)
                        {
                            if(lastExecutedCommand is RevertableCommand)
                            {
                                (lastExecutedCommand as RevertableCommand).HandleRevert(commandArguments);
                                commandExecutedSuccessfuly = true;
                            }
                            else
                            {
                                ConsoleIOManager.Instance.LogError("Last command is not reversible.");
                                commandExecutedSuccessfuly = false;
                            }
                        }
                        else
                        {
                            ConsoleIOManager.Instance.LogError("Last command was not executed successfully.");
                            commandExecutedSuccessfuly = false;
                        }
                    }
                    else
                    {
                        ConsoleIOManager.Instance.LogError("No command executed yet.");
                        commandExecutedSuccessfuly = false;
                    }

                    return commandExecutedSuccessfuly;
                }
            }

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

        private static CommandParsingManager instance;

        private Command lastExecutedCommand;

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

            // look for a command executor which recognizes user input as a command
            foreach (CommandExecutor commandExecutor in commandExecutors)
            {
                if (commandExecutor.IsValidCommand(userInputLowercase)) // command recognized
                {
                    string prefix = commandExecutor.GetCommandPrefix(userInputLowercase);

                    // if there's a space directly after prefix, split by prefix + space
                    // else split by prefix
                    string splitPrefix = userInputLowercase.Contains(prefix + " ") ? prefix + " " : prefix;
                    string commandArgumentString = 
                        userInputLowercase.GetSubstringAfterPrefix(splitPrefix);

                    // get command arguments array
                    string[] commandArguments;

                    if(commandArgumentString == string.Empty) // empty command args string
                    {
                        commandArguments = new string[0];
                    }
                    else // non-empty command args string
                    {
                        commandArguments = commandArgumentString.Split(" ");
                    }

                    this.lastExecutedCommand = commandExecutor.HandleCommand(prefix, commandArguments);

                    return;
                }         
            }

            // user input not recognized by any command executor
            logUnrecognizedCommandExecption(userInput);
        }

        private void logUnrecognizedCommandExecption(string userInput)
        {
            ConsoleIOManager.Instance.LogErrorFormat(
                false,
                "Unrecognized command: '{0}'.",             
                userInput);
        }
    }
}
