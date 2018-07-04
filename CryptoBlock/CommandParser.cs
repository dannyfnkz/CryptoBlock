﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoBlock.Utils;
using CryptoBlock.CMCAPI;
using CryptoBlock.IOManagement;

namespace CryptoBlock
{
    internal static class CommandParser
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
        
        static CommandParser()
        {

        }

        internal static void ParseCommand(string userInput)
        {
            try
            {
                Command command = Command.Parse(userInput);

                CommandExecutor.ExecuteCommand(command);
            }
            catch(Command.CommandException commandException)
            {
                ConsoleIOManager.Instance.LogError(commandException.Message);
            }

            // some padding
 ////           Console.WriteLine();
        }
    }
}
