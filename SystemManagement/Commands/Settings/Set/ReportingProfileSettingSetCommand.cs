using CryptoBlock.CommandHandling;
using CryptoBlock.ConfigurationManagement;
using CryptoBlock.ConfigurationManagement.Settings;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.ConfigurationManagement.ConfigurationManager;
using static CryptoBlock.IOManagement.ConsoleIOManager;

namespace CryptoBlock
{
    namespace SystemManagement.Commands.Settings.Set
    {
        /// <summary>
        /// represents a <see cref="SettingSetCommand"/> which lets user set the 
        /// <see cref="OutputReportingProfile"/>.
        /// </summary>
        internal class ReportingProfileSettingSetCommand : SettingSetCommand
        {       
            private const string PREFIX = "reporting profile";

            private const int MIN_NUMBER_OF_ARGUMENTS = 0;
            private const int MAX_NUMBER_OF_ARGUMENTS = 0;

            // menu prompting user to selected one of the pre-defined OutputReportingProfiles
            private static readonly MenuDialog outputReportingProfileMenuDialog =
                new MenuDialog(
                    "Please choose one of the following Reporting Profiles",
                    new string[]
                    {
                        OutputReportingProfile.GetOutputReportingProfile(0).MenuOptionLine,
                        OutputReportingProfile.GetOutputReportingProfile(1).MenuOptionLine,
                        OutputReportingProfile.GetOutputReportingProfile(2).MenuOptionLine
                    });

            internal ReportingProfileSettingSetCommand()
                : base(PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
            {

            }

            /// <summary>
            /// prompts user to select one of the pre-defined <see cref="OutputReportingProfile"/>s.
            /// </summary>
            /// <param name="commandArguments"></param>
            /// <returns>
            /// <seealso cref="Command.Execute(string[])"/>
            /// </returns>
            protected override bool Execute(string[] commandArguments)
            {
               bool commandExecutedSuccessfuly;

               // display menu allowing user to select desired reporting profile
               int userSelectedReportingProfileIndex = ConsoleIOManager.Instance.ShowMenuDialog(
                    outputReportingProfileMenuDialog,
                    ConsoleIOManager.eOutputReportType.CommandExecution);

                // menu indexing starts at 1, while profile indexing starts at 0
                userSelectedReportingProfileIndex--;

                // get OutputReportingProfile user selected by index
                OutputReportingProfile userSelectedOutputReportingProfile =
                    OutputReportingProfile.GetOutputReportingProfile(userSelectedReportingProfileIndex);

                try
                {
                    // set user selected OutputReportingProfile in SettingManager
                    ConfigurationManager.Instance.OutputReportingProfile =
                        userSelectedOutputReportingProfile;

                    // log success notice
                    ConsoleIOManager.Instance.LogNoticeFormat(
                        false,
                        eOutputReportType.CommandExecution,
                        "Reporting Profile '{0}' set successfully.",
                        userSelectedOutputReportingProfile.Title);

                    commandExecutedSuccessfuly = true;
                }
                catch(SettingsUpdateException settingsUpdateException)
                {
                    // log failure notice
                    ConsoleIOManager.Instance.LogError(
                        "An exception occurred while trying to update Reporting Profile setting",
                        eOutputReportType.CommandExecution);
                    ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage(
                        eOutputReportType.CommandExecution);

                    // log exception in error log file
                    ExceptionManager.Instance.LogException(settingsUpdateException);

                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
            }
        }
    }
}
