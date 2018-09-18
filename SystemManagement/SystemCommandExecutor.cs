using CryptoBlock.CommandHandling;
using CryptoBlock.CommandHandling.Arguments;
using CryptoBlock.IOManagement;
using CryptoBlock.SystemManagement.Commands;
using CryptoBlock.SystemManagement.Commands.Settings.Get;
using CryptoBlock.SystemManagement.Commands.Settings.Set;
using CryptoBlock.SystemManagement.Commands.Status;
using CryptoBlock.SystemManagement.Commands.UserDefinedCommands;
using CryptoBlock.Utils.InternetUtils;
using System;

namespace CryptoBlock
{
    namespace SystemManagement
    {
        /// <summary>
        /// handles executing system commands.
        /// </summary>
        public class SystemCommandExecutor : CommandExecutor
        {
            private const string COMMAND_TYPE = "System";

            public SystemCommandExecutor()
            {
                // add associations between commands and their prefixes
                AddCommandPrefixToCommandPair(
                    new InternetConnectivityStatusCommand());

                // OutputReportingProfile related commands
                AddCommandPrefixToCommandPair(
                    new ReportingProfileSettingSetCommand(),
                    new ReportingProfileSettingGetCommand());
                
                // UserDefinedCommand related commands
                AddCommandPrefixToCommandPair(
                    new UserDefinedCommandsAddCommand(),
                    new UserDefinedCommandsViewCommand(),
                    new UserDefinedCommandsRemoveCommand(),
                    new UserDefinedCommandsClearCommand());
            }

            public override string CommandType
            {
                get { return COMMAND_TYPE; }
            }
        }
    }
}