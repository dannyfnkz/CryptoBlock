using CryptoBlock.CommandHandling;
using CryptoBlock.CommandHandling.Arguments;
using CryptoBlock.IOManagement;
using CryptoBlock.SystemManagement.Commands;
using CryptoBlock.SystemManagement.Commands.Settings.Get;
using CryptoBlock.SystemManagement.Commands.Settings.Set;
using CryptoBlock.SystemManagement.Commands.Status;
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
                    new InternetConnectivityStatusCommand(),
                    new ReportingProfileSettingSetCommand(),
                    new ReportingProfileSettingGetCommand());
            }

            public override string CommandType
            {
                get { return COMMAND_TYPE; }
            }
        }
    }
}