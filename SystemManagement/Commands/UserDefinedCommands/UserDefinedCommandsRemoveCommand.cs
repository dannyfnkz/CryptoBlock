using CryptoBlock.CommandHandling;
using CryptoBlock.ConfigurationManagement;
using CryptoBlock.ExceptionManagement;
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
        internal class UserDefinedCommandsRemoveCommand : UserDefinedCommandsCommand
        {
            private const string PREFIX = "remove";

            private const int MIN_NUMBER_OF_ARGUMENTS = 1;
            private const int MAX_NUMBER_OF_ARGUMENTS = 1;

            internal UserDefinedCommandsRemoveCommand()
                : base(PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
            {

            }

            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                string userDefinedCommandAlias = commandArguments[0];

                // UserDefinedCommand with the specified alias exists
                if (ConfigurationManager.Instance.UserDefinedCommandExists(userDefinedCommandAlias))
                {
                    try
                    {
                        // remove UserDefinedCommand with specified alias
                        ConfigurationManager.Instance.RemoveUserDefinedCommand(userDefinedCommandAlias);

                        // log successful removal notice
                        string successNotice = string.Format(
                            "User Defined Command with alias '{0}' removed successfully.",
                            userDefinedCommandAlias);
                        LogCommandNotice(successNotice);

                        commandExecutedSuccessfuly = true;
                    }
                    catch (UserDefinedCommandsUpdateException userDefinedCommandsUpdateException)
                    {
                        UserDefinedCommandsCommand.HandleUserDefinedCommandsUpdateException(
                            userDefinedCommandsUpdateException);

                        commandExecutedSuccessfuly = false;
                    }
                }
                else // UserDefinedCommand with the specified alias does not exist
                {
                    string errorMessage = string.Format(
                      "User Defined Command with alias '{0}' not found.",
                      userDefinedCommandAlias);
                    Command.LogCommandError(errorMessage);

                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
            }
        }
    }

}
