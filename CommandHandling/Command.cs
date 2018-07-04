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
        public class Command
        {
            public class CommandException : Exception
            {
                public CommandException(string message)
                    : base(message)
                {

                }
            }

            public class InvalidCommandException : CommandException
            {
                public InvalidCommandException() : base("Invalid Command.")
                {

                }
            }

            public class InvalidCommandTypeException : CommandException
            {
                public InvalidCommandTypeException(string commandType)
                    : base(formatExceptionMessage(commandType))
                {

                }

                private static string formatExceptionMessage(string commandType)
                {
                    return string.Format("Invalid command Type: '{0}'.", commandType);
                }
            }

            public class WrongNumberOfArgumentsException : CommandException
            {
                public WrongNumberOfArgumentsException() : base("Wrong number of arguments.")
                {

                }
            }

            public enum eCommandType
            {
                ViewCoinTicker,
                ViewCoinListing
            }

            private static readonly Dictionary<eCommandType, string> commandTypeToString =
                new Dictionary<eCommandType, string>
                {
                { eCommandType.ViewCoinTicker, "view coin ticker" },
                { eCommandType.ViewCoinListing, "view coin listing" }
                };
            private static readonly Dictionary<eCommandType, int> commandTypeToMinNumberOfArguments =
                new Dictionary<eCommandType, int>
                {
                { eCommandType.ViewCoinTicker, 1 },
                { eCommandType.ViewCoinListing, 1 }
                };
            private static readonly Dictionary<eCommandType, int> commandTypeToMaxNumberOfArguments =
                new Dictionary<eCommandType, int>
                {
                { eCommandType.ViewCoinTicker, 1 },
                { eCommandType.ViewCoinListing, 1 }
                };

            private readonly eCommandType type;
            private string[] arguments;

            // expects a valid arguments array (matching command type)
            public Command(eCommandType type, string[] arguments)
            {
                this.type = type;
                this.arguments = arguments;
            }

            public eCommandType Type
            {
                get { return type; }
            }

            public string[] Arguments
            {
                get { return arguments; }
            }

            public int NumberOfArguments
            {
                get { return arguments.Length; }
            }

            public static Command Parse(string userInput)
            {
                eCommandType commandType = getCommandType(userInput);

                string lowerCaseUserInput = userInput.ToLower();

                // check if there's a space directly after command type
                if (lowerCaseUserInput.StartsWith(commandTypeToString[commandType] + " "))
                {
                    string parameterString = StringUtils.Substring(
                        lowerCaseUserInput,
                        commandTypeToString[commandType] + " ");

                    string[] arguments = StringUtils.Split(parameterString, " ");

                    // wrong number of arguments
                    if (arguments.Length < commandTypeToMinNumberOfArguments[commandType]
                        || arguments.Length > commandTypeToMaxNumberOfArguments[commandType])
                    {
                        throw new WrongNumberOfArgumentsException();
                    }

                    // empty argument in argument list
                    if (Array.IndexOf(arguments, string.Empty) > -1)
                    {
                        throw new WrongNumberOfArgumentsException();
                    }

                    Command command = new Command(commandType, arguments);

                    return command;
                }
                else // no space directly after command type
                {
                    throw new WrongNumberOfArgumentsException();
                }
            }

            private static eCommandType getCommandType(string userInput)
            {
                string lowercaseUserInput = userInput.ToLower();

                // check if command type is valid
                foreach (eCommandType commandType in commandTypeToString.Keys)
                {
                    // user entered a valid command type
                    if (lowercaseUserInput.StartsWith(commandTypeToString[commandType]))
                    {
                        return commandType;
                    }
                }

                // user entered an invalid command type
                string firstCommandWord = StringUtils.Split(userInput, " ")[0];

                throw new InvalidCommandTypeException(firstCommandWord);
            }
        }
    }
}