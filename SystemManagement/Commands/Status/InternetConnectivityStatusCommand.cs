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
    namespace SystemManagement.Commands.Status
    {
        /// <summary>
        /// <para>
        /// represents a <see cref="StatusCommand"/> which logs internet connectivity status to console.
        /// </para>
        /// <para>
        /// syntax: status connection
        /// </para>
        /// </summary>
        internal class InternetConnectivityStatusCommand : StatusCommand
        {
            // command sub-prefix
            private const string PREFIX = "connection";

            internal InternetConnectivityStatusCommand()
                : base(PREFIX)
            {

            }

            /// <summary>
            /// logs internet connectivity status to console.
            /// </summary>
            /// <seealso cref="InternetUtils.IsConnectedToInternet"/>
            /// <param name="commandArguments"></param>
            /// <returns>
            /// <seealso cref="Command.Execute(string[])"/>
            /// </returns>
            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                ConsoleIOManager.Instance.LogNotice(
                    "Checking internet connectivity ..",
                    ConsoleIOManager.eOutputReportType.CommandExecution);

                bool connectedToInternet = InternetUtils.IsConnectedToInternet();

                string notice = connectedToInternet ?
                    "Device is connected to internet."
                    : "No internet connection.";

                ConsoleIOManager.Instance.LogNotice(
                    notice,
                    ConsoleIOManager.eOutputReportType.CommandExecution);

                commandExecutedSuccessfuly = true;

                return commandExecutedSuccessfuly;
            }
        }
    }
}
