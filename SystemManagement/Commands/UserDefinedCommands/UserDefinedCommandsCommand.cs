using CryptoBlock.CommandHandling;
using CryptoBlock.CommandHandling.Arguments;
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
        /// <summary>
        /// represents a <see cref="Command"/> which handles the saved <see cref="UserDefinedCommands"/>
        /// repository.
        /// </summary>
        internal abstract class UserDefinedCommandsCommand : Command
        {
            private const string BASE_PREFIX = "user commands";

            internal UserDefinedCommandsCommand(
                string inheritingCommandPrefix,
                int minNumberOfArguments,
                int maxNumberOfArguments)
                : base(FormatPrefix(BASE_PREFIX, inheritingCommandPrefix))
            {
                base.commandArgumentConstraintList.Add(
                    new NumberOfArgumentsCommandArgumentConstraint(
                        minNumberOfArguments,
                        maxNumberOfArguments)
                );
            }

            /// <summary>
            /// logs details of thrown <paramref name="userDefinedCommandsUpdateException"/>
            /// to console and error log file.
            /// </summary>
            /// <param name="userDefinedCommandsUpdateException"></param>
            protected static void HandleUserDefinedCommandsUpdateException(
                UserDefinedCommandsUpdateException userDefinedCommandsUpdateException)
            {
                // log exception message to console
                Command.LogCommandError(
                    "An exception occurred while trying to update User Defined Commands data file.");
                Command.LogCommandReferToErrorLogFileMessage();

                // log exception to error log file
                ExceptionManager.Instance.LogException(userDefinedCommandsUpdateException);
            }
        }
    }
}
