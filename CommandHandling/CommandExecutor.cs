using CryptoBlock.Utils;
using CryptoBlock.Utils.Collections.List;
using CryptoBlock.Utils.Strings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoBlock
{
    namespace CommandHandling
    {
        /// <summary>
        /// handles executing <see cref="CommandType"/> commands.
        /// </summary>
        public abstract class CommandExecutor
        {
            /// <summary>
            /// thrown if command specified by user is not a valid <see cref="CommandType"/> command.
            /// </summary>
            public class InvalidCommandException : CommandExecutionException
            {
                public InvalidCommandException(string commandType)
                    : base(formatExceptionMessage(commandType))
                {

                }

                private static string formatExceptionMessage(string commandType)
                {
                    return string.Format("Invalid syntax for command type {0}.", commandType);
                }
            }

            /// <summary>
            ///  thrown when a requested command, associated with specified command prefix, does not exist.
            /// </summary>
            public class NoCommandAssociatedWithCommandPrefix : CommandExecutionException
            {

                public NoCommandAssociatedWithCommandPrefix(string commandPrefix)
                    : base(formatExceptionMessage(commandPrefix))
                {

                }

                private static string formatExceptionMessage(string commandPrefix)
                {
                    return string.Format(
                        "There's no command associated with commandp prefix '{0}'.",
                        commandPrefix);
                }
            }

            private readonly static List<string> reservedCommandPrefixes = new List<string>();

            // mapping of command prefixes to corresponding commands
            private readonly Dictionary<string, Command> commandPrefixToCommmand
                = new Dictionary<string, Command>();

    //        private string[] reservedCommandAliases;

            /// <summary>
            /// type of commands <see cref="CommandExecutor"/> handles.
            /// </summary>
            public abstract string CommandType
            {
                get;
            }

            public static string[] ReservedCommandPrefixes
            {
                get { return reservedCommandPrefixes.ToArray(); }
            }

            protected Command[] Commands
            {
                get { return commandPrefixToCommmand.Values.ToArray(); }
            }

            /// <summary>
            /// returns whether <paramref name="userInputLowercase"/> is a valid <see cref="CommandType"/> command.
            /// </summary>
            /// <param name="userInputLowercase"></param>
            /// <returns>
            /// true if <paramref name="userInputLowercase"/> is a valid <see cref="CommandType"/> command,
            /// else false
            /// </returns>
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

            /// <summary>
            /// returns command prefix of <see cref="Command"/> represented by <paramref name="userInputLowercase"/>.
            /// </summary>
            /// <seealso cref="StringUtils.GetPrefixIfStartsWith(string, string[])"/>
            /// <param name="userInputLowercase"></param>
            /// <returns>
            /// command prefix of <see cref="Command"/> represented by <paramref name="userInputLowercase"/>.
            /// </returns>
            /// <exception cref="InvalidCommandException">
            /// thrown if <paramref name="userInputLowercase"/> does not represent a valid <see cref="CommandType"/>
            /// command.
            /// </exception>
            public string GetCommandPrefix(string userInputLowercase)
            {
                // if user input starts with a recognized prefix, get prefix
                string prefix = userInputLowercase.GetPrefixIfStartsWith(
                    commandPrefixToCommmand.Keys.ToArray());

                if (prefix != null) // valid command
                {
                    return prefix;
                }
                else // invalid command
                {
                    throw new InvalidCommandException(CommandType);
                }
            }

            /// <summary>
            /// parses and handles command having <paramref name="commandPrefix"/>
            /// with associated <paramref name="commandArguments"/>.
            /// returns parsed <see cref="Command"/>.
            /// </summary>
            /// <param name="commandPrefix"></param>
            /// <param name="commandArguments"></param>
            /// <returns>
            /// parsed <see cref="Command"/> having <paramref name="commandPrefix"/>
            /// with associated <paramref name="commandArguments"/>
            /// </returns>
            /// <exception cref="InvalidCommandException">
            /// thrown if command is not a valid <see cref="CommandType"/> command.
            /// </exception>
            public Command HandleCommand(string commandPrefix, string[] commandArguments)
            {
                Command command;

                if (!IsValidCommand(commandPrefix))
                {
                    throw new InvalidCommandException(CommandType);
                }

                // get matching command
                command = GetCommand(commandPrefix);

                command.Handle(commandArguments);

                return command;
            }

            /// <summary>
            /// for every <see cref="Command"/> in <paramref name="commands"/>,
            /// adds association between <see cref="Command"/>.Prefix and <see cref="Command"/>.
            /// </summary>
            /// <seealso cref="AddCommandPrefixToCommandPair(string, Command)"/>
            /// <param name="commands"></param>
            protected void AddCommandPrefixToCommandPair(params Command[] commands)
            {
                foreach(Command command in commands)
                {
                    string commandPrefix = command.Prefix;
                    AddCommandPrefixToCommandPair(commandPrefix, command);
                }
            }

            /// <summary>
            /// for every (commandPrefix, <see cref="Command"/>) pair in <paramref name="prefixCommandPairs"/>,
            /// adds association between commandPrefix and <see cref="Command"/>.
            /// </summary>
            /// <seealso cref="AddCommandPrefixToCommandPair(string, Command)"/>
            /// <param name="prefixCommandPairs"></param>
            protected void AddCommandPrefixToCommandPair(
                IEnumerable<KeyValuePair<string, Command>> prefixCommandPairs)
            {
                foreach (KeyValuePair<string, Command> prefixCommandPair in prefixCommandPairs)
                {
                    AddCommandPrefixToCommandPair(prefixCommandPair.Key, prefixCommandPair.Value);
                }
            }

            /// <summary>
            /// adds association between commandPrefix and <see cref="Command"/> for
            /// (commandPrefix, <see cref="Command"/>) pair encapsulated in <paramref name="prefixCommandPair"/>.
            /// </summary>
            /// <seealso cref="AddCommandPrefixToCommandPair"/>
            /// <param name="prefixCommandPair"></param>
            protected void AddCommandPrefixToCommandPair(KeyValuePair<string, Command> prefixCommandPair)
            {
                AddCommandPrefixToCommandPair(prefixCommandPair.Key, prefixCommandPair.Value);
            }

            /// <summary>
            /// adds association between commandPrefix and <see cref="Command"/>.
            /// </summary>
            /// <param name="commandPrefix"></param>
            /// <param name="command"></param>
            protected void AddCommandPrefixToCommandPair(string commandPrefix, Command command)
            {
                this.commandPrefixToCommmand.Add(commandPrefix, command);

                // add command prefix as reserved command alias
                reservedCommandPrefixes.Add(commandPrefix);
            }

            /// <summary>
            /// returns <see cref="Command"/> associated with <paramref name="commandPrefix"/>.
            /// </summary>
            /// <param name="commandPrefix"></param>
            /// <returns>
            /// <see cref="Command"/> associated with <paramref name="commandPrefix"/>.
            /// </returns>
            /// <exception cref="NoCommandAssociatedWithCommandPrefix">
            /// thrown if no <see cref="Command"/> is associated with <paramref name="commandPrefix"/>.
            /// </exception>
            protected Command GetCommand(string commandPrefix)
            {
                // no command associated with commandPrefix
                if (!this.commandPrefixToCommmand.ContainsKey(commandPrefix))
                {
                    throw new NoCommandAssociatedWithCommandPrefix(commandPrefix);
                }

                return this.commandPrefixToCommmand[commandPrefix];
            }
        }
    }
}