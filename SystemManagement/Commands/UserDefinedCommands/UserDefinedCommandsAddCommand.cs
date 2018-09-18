using CryptoBlock.CommandHandling;
using CryptoBlock.ConfigurationManagement;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using CryptoBlock.SettingsManagement.SavedCommands;
using CryptoBlock.Utils.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.ConfigurationManagement.ConfigurationManager;

namespace CryptoBlock
{
    namespace SystemManagement.Commands.UserDefinedCommands
    {
        internal class UserDefinedCommandsAddCommand : UserDefinedCommandsCommand
        {
            private const string PREFIX = "add";

            private const int MIN_NUMBER_OF_ARGUMENTS = 2;
            private const int MAX_NUMBER_OF_ARGUMENTS = 2;

            internal UserDefinedCommandsAddCommand()
                : base(PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
            {

            }

            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                string userDefinedCommandAlias = commandArguments[0];
                string userDefinedCommandString = commandArguments[1];

                // returns whether userDefinedCommandAlias starts with a reserved command prefix
                // supplied as argument
                Predicate<string> userDefinedCommandAliasContainsReservedCommandPrefix
                    = reservedCommandPrefix => userDefinedCommandAlias.StartsWith(reservedCommandPrefix);

                // UserDefinedCommand with the specified alias already exists
                if (ConfigurationManager.Instance.UserDefinedCommandExists(userDefinedCommandAlias))
                {
                    string errorMessage = string.Format(
                        "Cannot add command: A user defined command with alias '{0}' already exists.",
                        userDefinedCommandAlias);
                    Command.LogCommandError(errorMessage);

                    commandExecutedSuccessfuly = false;
                }
                // command alias starts with a reserved command prefix
                else if(CommandExecutor.ReservedCommandPrefixes.TrueForAny(
                    userDefinedCommandAliasContainsReservedCommandPrefix))
                {
                    // get reserved command prefix
                    string reservedCommandPrefix =
                        CommandExecutor.ReservedCommandPrefixes.FirstElementWhichSatisfies(
                            userDefinedCommandAliasContainsReservedCommandPrefix);

                    // log error message
                    string errorMessage = string.Format(
                        "Cannot add command: command prefix '{0}' is reserved.",
                        reservedCommandPrefix);
                    Command.LogCommandError(errorMessage);

                    commandExecutedSuccessfuly = false;
                } 
                else // legal command alias
                {
                    try
                    {
                        // add UserDefinedCommand to ConfigurationManager
                        UserDefinedCommand savedCommand = new UserDefinedCommand(
                            userDefinedCommandAlias,
                            userDefinedCommandString);
                        ConfigurationManager.Instance.AddUserDefinedCommand(savedCommand);

                        // log success notice
                        string successNotice = string.Format(
                            "User Defined Command with alias '{0}' was added successfully.",
                            userDefinedCommandAlias);
                        Command.LogCommandNotice(successNotice);

                        commandExecutedSuccessfuly = true;
                    }
                    catch(UserDefinedCommandsUpdateException userDefinedCommandsUpdateException)
                    {
                        UserDefinedCommandsCommand.HandleUserDefinedCommandsUpdateException(
                            userDefinedCommandsUpdateException);

                        commandExecutedSuccessfuly = false;
                    }
                }

                return commandExecutedSuccessfuly;
            }
        }
    }

}
