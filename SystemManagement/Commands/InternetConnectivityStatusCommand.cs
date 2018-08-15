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
    }
}
