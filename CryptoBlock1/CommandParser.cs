using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoBlock.Utils;
using CryptoBlock.CMCAPI;

namespace CryptoBlock
{
    static class CommandParser
    {
        internal class CommandParseException : Exception
        {
            internal CommandParseException(string message) : base(message)
            {

            }
        }

        internal class InvalidCommandException : CommandParseException
        {
            internal InvalidCommandException() : base("Invalid command.")
            {

            }
        }

        internal class WrongNumberOfArgumentsException : CommandParseException
        {
            internal WrongNumberOfArgumentsException() : base("Wrong number of arguments.")
            {

            }
        }

        internal static void ParseCommand(string command)
        {
            string lowerCaseCommand = command.ToLower();

            if (lowerCaseCommand.StartsWith("view data "))
            {
                string[] parameters = StringUtils.Split(StringUtils.Substring(lowerCaseCommand, "view data "), " ");
                parseViewDataCommand(parameters);
            }
            else
            {
                ConsoleUtils.LogLine("Invalid Command.");
            }

            // some padding
            Console.WriteLine();
        }

        private static void parseViewDataCommand(string[] parameters)
        {
            if(parameters.Length != 1)
            {
                ConsoleUtils.LogLine("Wrong number of arguments.");
            }

            string coinNameOrSymbol = parameters[0];

            CommandExecutor.ExecuteViewDataCommand(coinNameOrSymbol);
        }
    }
}
