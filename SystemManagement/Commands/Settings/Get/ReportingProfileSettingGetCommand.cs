using CryptoBlock.CommandHandling;
using CryptoBlock.ConfigurationManagement;
using CryptoBlock.ConfigurationManagement.Settings;
using CryptoBlock.IOManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.IOManagement.ConsoleIOManager;

namespace CryptoBlock
{
    namespace SystemManagement.Commands.Settings.Get
    {
        /// <summary>
        /// represents a <see cref="SettingGetCommand"/> which logs to console details about
        /// the currently set <see cref="OutputReportingProfile"/>.
        /// </summary>
        internal class ReportingProfileSettingGetCommand : SettingGetCommand
        {
            private const string PREFIX = "reporting profile";

            private const int MIN_NUMBER_OF_ARGUMENTS = 0;
            private const int MAX_NUMBER_OF_ARGUMENTS = 0;

            internal ReportingProfileSettingGetCommand()
                : base(PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
            {

            }

            /// <summary>
            /// logs to console details about
            /// the currently set <see cref="OutputReportingProfile"/>.
            /// </summary>
            /// <param name="commandArguments"></param>
            /// <returns>
            /// <seealso cref="Command.Execute(string[])"/>
            /// </returns>
            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                // get current OutputReportingProfile 
                OutputReportingProfile outputReportingProfile =
                    ConfigurationManager.Instance.OutputReportingProfile;

                // log OutputReportingProfile title to console
                string notice = string.Format("" +
                    "Current Reporting Profile: '{0}'.",
                    outputReportingProfile.Title);
                ConsoleIOManager.Instance.LogNotice(notice, eOutputReportType.CommandExecution);

                commandExecutedSuccessfuly = true;

                return commandExecutedSuccessfuly;
            }
        }
    }
}

