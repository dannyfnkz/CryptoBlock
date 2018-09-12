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
        internal static class PortfolioCommandUtils
        {
            // command prefix
            private const string PREFIX = "portfolio";

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
            internal static string FormatPrefix(string inheritingCommandPrefix)
            {
                return PREFIX + " " + inheritingCommandPrefix;
            }
        }
    }
}