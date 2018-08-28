using CryptoBlock.IOManagement;
using CryptoBlock.Utils.InternetUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace SystemManagement.Commands
    {
        /// <summary>
        /// <para>
        /// prints internet connectivity status.
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
            /// prints internet connectivity status.
            /// returns whether command was executed successfully.
            /// </summary>
            /// <seealso cref="InternetUtils.IsConnectedToInternet"/>
            /// <param name="commandArguments"></param>
            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                ConsoleIOManager.Instance.LogNotice("Checking internet connectivity ..");

                bool connectedToInternet = InternetUtils.IsConnectedToInternet();

                string notice = connectedToInternet ?
                    "Device is connected to internet."
                    : "No internet connection.";

                ConsoleIOManager.Instance.LogNotice(notice);

                commandExecutedSuccessfuly = true;

                return commandExecutedSuccessfuly;
            }
        }
    }
}
