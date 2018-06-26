using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    internal class Command
    {
        internal class CommandException : Exception
        {
            internal CommandException(string message)
                : base(message)
            {

            }
        }

        internal class InvalidCommandException : CommandException
        {
            internal InvalidCommandException() : base("Invalid Command.")
            {

            }
        }

        internal class InvalidCommandTypeException : CommandException
        {
            internal InvalidCommandTypeException(string commandType) 
                : base(formatExceptionMessage(commandType))
            {

            }

            private static string formatExceptionMessage(string commandType)
            {
                return string.Format("Invalid command Type: '{0}'.", commandType);
            }
        }

        internal class WrongNumberOfArgumentsException : CommandException
        {
            internal WrongNumberOfArgumentsException() : base("Wrong number of arguments.")
            {

            }
        }

        internal enum eCommandType
        {
            ViewCoinData,
            ViewCoinListing
        }

        private static readonly Dictionary<eCommandType, string> commandTypeToString =
            new Dictionary<eCommandType, string>();
        private static readonly Dictionary<eCommandType, int> commandTypeToMinNumberOfArguments =
            new Dictionary<eCommandType, int>();
        private static readonly Dictionary<eCommandType, int> commandTypeToMaxNumberOfArguments =
            new Dictionary<eCommandType, int>();

        static Command()
        {
            // init commandTypeToString dictionary
            commandTypeToString[eCommandType.ViewCoinData] = "view coin data";
            commandTypeToString[eCommandType.ViewCoinListing] = "view coin listing";

            // init commandTypeToMinNumberOfArguments, commandTypeToMaxNumberOfArguments dictionaries
            commandTypeToMinNumberOfArguments[eCommandType.ViewCoinData] = 1;
            commandTypeToMaxNumberOfArguments[eCommandType.ViewCoinData] = 1;

            commandTypeToMinNumberOfArguments[eCommandType.ViewCoinListing] = 1;
            commandTypeToMaxNumberOfArguments[eCommandType.ViewCoinListing] = 1;
        }

        private readonly eCommandType type;
        private string[] arguments;

        // expects a valid arguments array (matching command type)
        internal Command(eCommandType type, string[] arguments)
        {
            this.type = type;
            this.arguments = arguments;
        }

        internal eCommandType Type
        {
            get { return type; }
        }

        internal string[] Arguments
        {
            get { return arguments; }
        }

        internal int NumberOfArguments
        {
            get { return arguments.Length; }
        }

        internal static Command Parse(string userInput)
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
