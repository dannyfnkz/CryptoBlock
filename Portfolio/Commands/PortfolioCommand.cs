using CryptoBlock.CommandHandling;
using CryptoBlock.CommandHandling.Arguments;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.IOManagement.ConsoleIOManager;
using static CryptoBlock.PortfolioManagement.PortfolioManager;

namespace CryptoBlock
{
    namespace PortfolioManagement.Commands
    {
        /// <summary>
        /// represents an executable portfolio command.
        /// </summary>
        internal abstract class PortfolioCommand : Command
        {
            // command prefix
            private const string PREFIX = "portfolio";

            internal PortfolioCommand(
                string inheritingCommandPrefix,
                int minNumberOfArguments,
                int maxNumberOfArguments)
                : base(formatPrefix(inheritingCommandPrefix))
            {
                base.commandArgumentConstraintList.Add(
                    new NumberOfArgumentsCommandArgumentConstraint(
                            minNumberOfArguments,
                            maxNumberOfArguments)
                );
            }

            /// <summary>
            /// logs an error message to console and logs <paramref name="databaseCommunicationException"/>
            /// to error log file.
            /// </summary>
            /// <param name="databaseCommunicationException"></param>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="ExceptionManager.LogException(Exception)"/>
            /// </exception>
            static internal void HandleDatabaseCommunicationException(
                DatabaseCommunicationException databaseCommunicationException)
            {
                ConsoleIOManager.Instance.LogError(
                    "An error occurred while trying to access portfolio database.",
                    eOutputReportType.CommandExecution);
                ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage(
                    eOutputReportType.CommandExecution);
                ExceptionManager.Instance.LogException(databaseCommunicationException);
            }

            /// <summary>
            /// returns command prefix formulated by concatenating <paramref name="inheritingCommandPrefix"/> to
            /// <see cref="Command.Prefix"/>.
            /// </summary>
            /// <param name="inheritingCommandPrefix"></param>
            /// <returns>
            /// command prefix formulated by concatenating <paramref name="inheritingCommandPrefix"/> to
            /// <see cref="Command.Prefix"/>
            /// </returns>
            private static string formatPrefix(string inheritingCommandPrefix)
            {
                return PREFIX + " " + inheritingCommandPrefix;
            }
        }
    }
}