using CryptoBlock.CommandHandling;
using CryptoBlock.ConfigurationManagement;
using CryptoBlock.IOManagement;
using CryptoBlock.SettingsManagement.SavedCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace SystemManagement.Commands.UserDefinedCommands
    {
        internal class UserDefinedCommandsViewCommand : UserDefinedCommandsCommand
        {
            private const string PREFIX = "view";

            private const int MIN_NUMBER_OF_ARGUMENTS = 0;
            private const int MAX_NUMBER_OF_ARGUMENTS = 1;

            internal UserDefinedCommandsViewCommand()
                : base(PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
            {

            }

            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                int numOfCommandArguments = commandArguments.Length;

                string notice = null;
                StringBuilder UserDefinedCommandDataStringBuilder = new StringBuilder();

                if (numOfCommandArguments == 0) // user did not specify a particular alias
                {
                    notice = "Existing User Defined Commands:";

                    // display all UserDefinedCommands aliases with their corresponding command strings

                    // no UserDefinedCommands available
                    if(ConfigurationManager.Instance.UserDefinedCommands.Length == 0)
                    {
                        UserDefinedCommandDataStringBuilder.Append("No User Defined Commands were added.");
                    }
                    else // ConfigurationManager.Instance.UserDefinedCommands.Length > 0
                    {
                        for(int i = 0; i < ConfigurationManager.Instance.UserDefinedCommands.Length; i++)
                        {
                            UserDefinedCommand userDefinedCommand =
                                 ConfigurationManager.Instance.UserDefinedCommands[i];
                            UserDefinedCommandDataStringBuilder.AppendFormat(
                                 "{0}. {1} - '{2}'",
                                 i + 1,
                                 userDefinedCommand.Alias,
                                 userDefinedCommand.CommandString);

                            if (i < ConfigurationManager.Instance.UserDefinedCommands.Length - 1)
                            {
                                UserDefinedCommandDataStringBuilder.Append(Environment.NewLine);
                            }
                        }
                    }

                    commandExecutedSuccessfuly = true;
                }
                else // numOfCommandArguments == 1 - user specified a paritcular alias
                {
                    string userDefinedCommandAlias = commandArguments[0];

                    // UserDefinedCommand with specified alias found
                    if (ConfigurationManager.Instance.UserDefinedCommandExists(userDefinedCommandAlias))
                    {
                        UserDefinedCommand userDefinedCommand =
                            ConfigurationManager.Instance.GetUserDefinedCommand(userDefinedCommandAlias);

                        notice = string.Format(
                            "User Defined Command for alias '{0}':",
                            userDefinedCommandAlias);
                        UserDefinedCommandDataStringBuilder.AppendFormat(
                            "{0} - '{1}'",
                            userDefinedCommand.Alias,
                            userDefinedCommand.CommandString);

                        commandExecutedSuccessfuly = true;
                    }
                    else // UserDefinedCommand with specified alias not found
                    {
                        // log error message
                        string errorMessage = string.Format(
                            "User Defined Command with specified alias '{0}' not found.",
                            userDefinedCommandAlias);
                        Command.LogCommandError(errorMessage);

                        commandExecutedSuccessfuly = false;
                    }
                }

                // UserDefinedCommand corresponding to specified alias exists,
                // or user requested to view all UserDefinedCommands
                if (commandExecutedSuccessfuly)
                {
                    string userDefinedCommandData = UserDefinedCommandDataStringBuilder.ToString();

                    Command.LogCommandNotice(notice);
                    Command.PrintCommandData(userDefinedCommandData);
                }

                return commandExecutedSuccessfuly;
            }
        }
    }
}

