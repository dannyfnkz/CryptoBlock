using System;
using CryptoBlock.IOManagement;

namespace CryptoBlock
{
    namespace CommandHandling
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

            static CommandParser()
            {

            }

            public static void ParseCommand(string userInput)
            {
                try
                {
                    Command command = Command.Parse(userInput);

                    CommandExecutor.ExecuteCommand(command);
                }
                catch (Command.CommandException commandException)
                {
                    ConsoleIOManager.Instance.LogError(commandException.Message);
                }

                // some padding
                ////           Console.WriteLine();
            }
        }
    }

}
