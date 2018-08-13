using CryptoBlock.CommandHandling;
using CryptoBlock.CommandHandling.Arguments;
using CryptoBlock.IOManagement;
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
            /// <summary>
            /// represents an executable system command.
            /// </summary>
            private abstract class SystemCommand : Command
            {
                internal SystemCommand(string prefix, int minNumberOfArguments, int maxNumberOfArguments)
                    : base(prefix)
                {
                    base.commandArgumentConstraintList.Add(
                        new NumberOfArgumentsCommandArgumentConstraint(
                            minNumberOfArguments,
                            maxNumberOfArguments)
                        );
                }
            }

            /// <summary>
            /// represents an executable status command.
            /// </summary>
            private abstract class StatusCommand : Command
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 0;
                private const int MAX_NUMBER_OF_ARGUMENTS = 0;

                private const string PREFIX = "status";

                internal StatusCommand(string inheritingCommandPrefix)
                    : base(formatPrefix(inheritingCommandPrefix))
                {
                    base.commandArgumentConstraintList.Add(
                        new NumberOfArgumentsCommandArgumentConstraint(
                            MIN_NUMBER_OF_ARGUMENTS,
                            MAX_NUMBER_OF_ARGUMENTS)
                        );
                }

                /// <summary>
                /// returns command prefix formulated by concatenating <paramref name="inheritingCommandPrefix"/> to
                /// <see cref="Command.Prefix"/>.
                /// </summary>
                /// <param name="inheritingCommandPrefix"></param>
                /// <returns>
                /// command prefix formulated by concatenating <paramref name="inheritingCommandPrefix"/> to
                /// <see cref="Command.Prefix"/>.
                /// </returns>
                private static string formatPrefix(string inheritingCommandPrefix)
                {
                    return PREFIX + " " + inheritingCommandPrefix;
                }
            }

            /// <summary>
            /// <para>
            /// prints internet connectivity status.
            /// </para>
            /// <para>
            /// syntax: status connection
            /// </para>
            /// </summary>
            private class InternetConnectivityStatusCommand : StatusCommand
            {
                // command sub-prefix
                private const string PREFIX = "connection";

                internal InternetConnectivityStatusCommand()
                    : base(PREFIX)
                {

                }

                /// <summary>
                /// prints internet connectivity status
                /// </summary>
                /// <seealso cref="InternetUtils.IsConnectedToInternet"/>
                /// <param name="commandArguments"></param>
                public override void ExecuteCommand(string[] commandArguments)
                {
                    bool commandArgumentsValid = base.CheckCommandArgumentConstraints(commandArguments);

                    if (!commandArgumentsValid)
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
                // add associations between commands and their prefixes
                AddCommandPrefixToCommandPair(
                    new InternetConnectivityStatusCommand());
            }

            public override string CommandType
            {
                get { return COMMAND_TYPE; }
            }
        }
    }
}