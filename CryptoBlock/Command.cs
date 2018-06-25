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
            internal WrongNumberOfArgumentsException() : base("Wrong numbe of arguments.")
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

        static Command()
        {
            commandTypeToString[eCommandType.ViewCoinData] = "view coin data";
            commandTypeToString[eCommandType.ViewCoinListing] = "view coin listing";
        }

        private readonly eCommandType type;
        private string[] parameters;

        internal Command(eCommandType type, string[] parameters)
        {
            this.type = type;
            this.parameters = parameters;
        }

        internal eCommandType Type
        {
            get { return type; }
        }

        internal string[] Parameters
        {
            get { return parameters; }
        }

        internal static Command Parse(string userInput)
        {
            string lowerCaseUserInput = userInput.ToLower();

            // check if command user entered is valid
            foreach (eCommandType commandType in commandTypeToString.Keys)
            {
                // user entered a valid command type
                if (lowerCaseUserInput.StartsWith(commandTypeToString[commandType]))
                {
                    // no space directly after command name
                    if (!lowerCaseUserInput.StartsWith(commandTypeToString[commandType] + " "))
                    {
                        throw new WrongNumberOfArgumentsException();
                    }
                    else // valid command type with space directly after
                    {
                        string parameterString = StringUtils.Substring(
                            lowerCaseUserInput,
                            commandTypeToString[commandType] + " ");

                        string[] parameters = StringUtils.Split(parameterString, " ");

                        Command command = new Command(commandType, parameters);

                        return command;
                    }
                }
            }

            // user entered an invalid command type
            string firstCommandWord = StringUtils.Split(userInput, " ")[0];

            throw new InvalidCommandTypeException(firstCommandWord);
        }
    }
}
