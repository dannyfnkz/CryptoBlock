using CryptoBlock.Utils;
using CryptoBlock.Utils.Collections;
using CryptoBlock.Utils.IO.ConsoleIO;
using System;

namespace CryptoBlock
{
    namespace IOManagement
    {
        /// <summary>
        /// manages console input / output operations.
        /// </summary>
        /// <remarks>
        /// only one instance of <see cref="ConsoleIOHandler"/> exists in the application.
        /// </remarks>
        /// <seealso cref="Utils.ConsoleIOHandler"/>
        public class ConsoleIOManager : ConsoleIOHandler
        {
            /// <summary>
            /// report type of output being logged to console.
            /// </summary>
            public enum eOutputReportType
            {
                ExceptionLog, System, SystemCritical, CommandExecution
            }

            private static readonly eOutputReportType[] DEFAULT_OUTPUT_REPORT_TYPES =
                new eOutputReportType[]
                {
                    eOutputReportType.SystemCritical,
                    eOutputReportType.CommandExecution  
                };

            private static readonly ConsoleIOManager instance = new ConsoleIOManager();

            private eOutputReportType[] outputReportTypes = DEFAULT_OUTPUT_REPORT_TYPES;

            public static ConsoleIOManager Instance
            {
                get { return instance; }
            }

            /// <summary>
            /// list of <see cref="eOutputReportType"/>s, where only outputs associated
            /// with an <see cref="eOutputReportType"/> contained in the list are displayed.
            /// other outputs are ignored.
            /// </summary>
            public eOutputReportType[] OutputReportTypes
            {
                get { return outputReportTypes; }
                set
                {
                    outputReportTypes = value ?? throw new ArgumentNullException("OutputReportTypes");
                }
            }

            /// <summary>
            /// displayed a 'press any key to continue' prompt, then blocks until a console key press
            /// is registered.
            /// </summary>
            /// <seealso cref="LogNotice(string, eOutputReportType, bool)"/>
            /// <seealso cref="ConsoleIOHandler.ReadKey"/>
            /// <param name="outputReportType"></param>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="LogNotice(string, eOutputReportType, bool)"/>
            /// </exception>
            public void ShowPressAnyKeyToContinueDialog(eOutputReportType outputReportType)
            {
                string dialogPromptMessage = "Press any key to continue ..";
                LogNotice(dialogPromptMessage, outputReportType);

                // wait for key press
                base.ReadKey();
            }

            /// <summary>
            /// logs notice message to console, replacing each format item in <paramref name="format"/> with 
            /// the string representation of the corresponding object in <paramref name="args"/>.
            /// </summary>
            /// <seealso cref="LogNotice(string, eOutputReportType, bool)"/>
            /// <param name="flushOutputBuffer"></param>
            /// <param name="outputReportType"
            /// <param name="format">a composite format string</param>
            /// <param name="args">object array containing zero or more objects to format</param>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="string.Format(string, object[])"/>
            /// </exception>
            /// <exception cref="FormatException">
            /// <seealso cref="string.Format(string, object[])"/>
            /// </exception>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="LogNotice(string, bool)"/>
            /// </exception>
            public void LogNoticeFormat(
                bool flushOutputBuffer,
                eOutputReportType outputReportType,
                string format,
                params object[] args)
            {
                string noticeMessage = string.Format(format, args);
                LogNotice(noticeMessage, outputReportType, flushOutputBuffer);
            }

            /// <summary>
            /// logs specified <paramref name="noticeMessage"/> to console.
            /// </summary>
            /// <seealso cref="formatLogMessage(string)"/>
            /// <seealso cref="logoutput(string, eOutputReportType, bool)"/>
            /// <param name="noticeMessage"></param>
            /// <param name="outputReportType"></param>
            /// <param name="flushOutputBuffer"></param>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="logoutput(string, eOutputReportType, bool)"/>
            /// </exception>
            public void LogNotice(
                string noticeMessage,
                eOutputReportType outputReportType, 
                bool flushOutputBuffer = false)
            {
                string outputString = formatLogMessage(noticeMessage);
                logoutput(outputString, outputReportType, flushOutputBuffer);
            }

            /// <summary>
            /// logs error message to console, replacing each format item in <paramref name="format"/> with 
            /// the string representation of the corresponding object in <paramref name="args"/>.
            /// </summary>
            /// <seealso cref="String.Format(string, object[])"/>
            /// <seealso cref="LogError(string, eOutputReportType, bool)"/>
            /// <param name="flushOutputBuffer"></param>
            /// <param name="outputReportType"
            /// <param name="format">a composite format string</param>
            /// <param name="args">object array containing zero or more objects to format</param>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="string.Format(string, object[])"/>
            /// </exception>
            /// <exception cref="FormatException">
            /// <seealso cref="string.Format(string, object[])"/>
            /// </exception>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="LogError(string, eOutputReportType, bool)"/>
            /// </exception>
            public void LogErrorFormat(
                bool flushOutputBuffer,
                eOutputReportType outputReportType,
                string format,
                params object[] args)
            {
                string errorMessage = string.Format(format, args);
                LogError(errorMessage, outputReportType, flushOutputBuffer);
            }

            /// <summary>
            /// logs specified <paramref name="errorMessage"/> to console.
            /// </summary>
            /// <seealso cref="formatLogMessage(string)"/>
            /// <seealso cref="logoutput(string, eOutputReportType, bool)"/> 
            /// <param name="errorMessage"></param>
            /// <param name="outputReportType"></param>
            /// <param name="flushOutputBuffer"></param>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="logoutput(string, eOutputReportType, bool)"/>
            /// </exception>
            public void LogError(
                string errorMessage,
                eOutputReportType outputReportType
                , bool flushOutputBuffer = false)
            {
                string outputString = formatLogMessage(errorMessage);
                logoutput(outputString, outputReportType, flushOutputBuffer);
            }

            /// <summary>
            /// prints data message to console, replacing each format item in <paramref name="format"/> with 
            /// the string representation of the corresponding object in <paramref name="args"/>.
            /// </summary>
            /// <seealso cref="String.Format(string, object[])"/>
            /// <seealso cref="PrintData(string, eOutputReportType, bool)"/>
            /// <param name="flushOutputBuffer"></param>
            /// <param name="outputReportType"></param>
            /// <param name="format"></param>
            /// <param name="args"></param>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="PrintData(string, eOutputReportType, bool)"/>
            /// </exception>
            public void PrintDataFormat(
                bool flushOutputBuffer,
                eOutputReportType outputReportType,
                string format,
                params object[] args)
            {
                string dataString = string.Format(format, args);
                PrintData(dataString, outputReportType, flushOutputBuffer);
            }

            /// <summary>
            /// prints specified <paramref name="data"/> to console.
            /// </summary>
            /// <seealso cref="logoutput(string, eOutputReportType, bool)"/>
            /// <param name="data"></param>
            /// <param name="outputReportType"></param>
            /// <param name="flushOutputBuffer"></param>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="logoutput(string, eOutputReportType, bool)"/>
            /// </exception>
            public void PrintData(
                string data, 
                eOutputReportType outputReportType,
                bool flushOutputBuffer = false)
            {
                string outputString = data;
                logoutput(outputString, outputReportType, flushOutputBuffer);
            }

            /// <summary>
            /// prints a new line char sequence to Console.
            /// </summary>
            /// <seealso cref="logoutput(string, eOutputReportType, bool)"/>
            /// <param name="outputReportType"
            /// <param name="flushOutputBuffer"></param>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="logoutput(string, eOutputReportType, bool)"/>
            /// </exception>
            public void PrintNewLine(eOutputReportType outputReportType, bool flushOutputBuffer = false)
            {
                string outputString = string.Empty;
                logoutput(outputString, outputReportType, flushOutputBuffer);
            }

            /// <summary>
            /// synchroniously displays a confirmation dialog with <paramref name="promptMessage"/>,
            /// allowing user to choose either 'yes' or 'no', and returns user choice.
            /// </summary>
            /// <seealso cref="ConsoleIOHandler.ReadLine()"/>
            /// <param name="promptMessage"></param>
            /// <returns>
            /// true if user chose 'yes',
            /// else false
            /// </returns>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="LogNotice(string, bool)"/>
            /// </exception>
            public bool ShowConfirmationDialog(string promptMessage, eOutputReportType outputReportType)
            {
                bool userChoice;

                // construct dialog display message
                string dialogPromptMessage = promptMessage + " (Y/N)";
                LogNotice(dialogPromptMessage, outputReportType);

                // read user input
                string userInput = base.ReadLine().ToLower();

                // keep requesting input so long as it's not valid
                while(userInput != "y" && userInput != "n")
                {
                    LogNotice("Invalid input, please select 'Y' or 'N'", outputReportType);
                    userInput = base.ReadLine().ToLower();
                }

                userChoice = userInput == "y" ? true : false;

                return userChoice;
            }

            /// <summary>
            /// logs display string of <paramref name="menuDialog"/> to console,
            /// blocks until user selects a menu option,
            /// and returns selected menu option index.
            /// </summary>
            /// <seealso cref="LogNotice(string, eOutputReportType, bool)"/>
            /// <seealso cref="int.TryParse(string, out int)"/>
            /// <param name="menuDialog"></param>
            /// <param name="outputReportType"></param>
            /// <returns>
            /// menu option index user selected
            /// </returns>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="LogNotice(string, eOutputReportType, bool)"/>
            /// </exception>
            public int ShowMenuDialog(MenuDialog menuDialog, eOutputReportType outputReportType)
            {
                // display menu dialog
                LogNotice(menuDialog.DisplayString, outputReportType);

                // read user input
                string userInput = base.ReadLine();

                // parse int from user input
                bool parseSuccessful = int.TryParse(userInput, out int userSelectedOptionIndex);

                // keep requesting input so long as it's not valid
                while (!(parseSuccessful && menuDialog.IsValidOptionIndex(userSelectedOptionIndex)))
                {
                    LogNoticeFormat(
                        false,
                        outputReportType,
                        "Invalid input, please select an option in range [{0} - {1}]:", 
                         menuDialog.MinValidOptionIndex,
                         menuDialog.MaxValidOptionIndex);

                    userInput = base.ReadLine();
                    parseSuccessful = int.TryParse(userInput, out userSelectedOptionIndex);
                }

                return userSelectedOptionIndex;
            }

            /// <summary>
            /// prints specified <paramref name="output"/> to console.
            /// </summary>
            /// <param name="output"></param>
            /// <param name="outputReportType"></param>
            /// <param name="flushOutputBuffer"></param>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="ConsoleIOHandler.QueueOutput(string, bool)"/>
            /// </exception>
            private void logoutput(
                string output,
                eOutputReportType outputReportType,
                bool flushOutputBuffer)
            {
                string outputWithNewline = output + Environment.NewLine;
                if (this.OutputReportTypes.Contains(outputReportType))
                {
                    base.QueueOutput(outputWithNewline, flushOutputBuffer);
                }
            }

            /// <summary>
            /// returns <paramref name="message"/> formatted as a log message.
            /// </summary>
            /// <seealso cref="Utils.DateTimeUtils.GetLogMessage(string)"/>
            /// <param name="message"></param>
            /// <returns>
            /// <paramref name="message"/> formatted as a log message
            /// </returns>
            private string formatLogMessage(string message)
            {
                return DateTimeUtils.FormatLogMessage(message);
            }
        }
    }
}
