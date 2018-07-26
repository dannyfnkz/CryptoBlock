using CryptoBlock.CommandHandling;
using CryptoBlock.IOManagement;
using CryptoBlock.Utils.InternetUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace SystemManagement
    {
        public class SystemCommandExecutor : CommandExecutor
        {
            private abstract class SystemCommand : Command
            {
                internal SystemCommand(string prefix, int minNumberOfArguments, int maxNumberOfArguments)
                    : base(prefix, minNumberOfArguments, maxNumberOfArguments)
                {

                }
            }

            private abstract class StatusCommand : Command
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 0;
                private const int MAX_NUMBER_OF_ARGUMENTS = 0;

                private const string PREFIX = "status";

                internal StatusCommand(string subPrefix)
                    : base(formatPrefix(subPrefix), MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }

                private static string formatPrefix(string subPrefix)
                {
                    return PREFIX + " " + subPrefix;
                }
            }

            private class ConnectivityStatusCommand : StatusCommand
            {
                private const string PREFIX = "connection";

                internal ConnectivityStatusCommand()
                    : base(PREFIX)
                {

                }

                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleInvalidNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }

                    ConsoleIOManager.Instance.LogNotice("Checking internet connectivity ..");

                    bool connectedToInternet = InternetUtils.IsConnectedToInternet();

                    string notice = connectedToInternet ?
                        "Device is connected to internet."
                        : "No internet connection.";

                    ConsoleIOManager.Instance.LogNotice(notice);
                }
            }

            private const string COMMAND_TYPE = "System";

            public SystemCommandExecutor()
            {
                // populate commandPrefixToCommmand dictionary with (prefix, command) pairs
                AddPrefixToCommandPair(
                    new ConnectivityStatusCommand());
            }

            protected override string GetCommandType()
            {
                return COMMAND_TYPE;
            }
        }
    }
}