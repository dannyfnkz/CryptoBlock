using CryptoBlock.Utils;
using CryptoBlock.Utils.IO.ConsoleIO;
using System;

namespace CryptoBlock
{
    namespace IOManagement
    {
        /// <summary>
        /// manages Console input / output operations.
        /// </summary>
        /// <remarks>
        /// only one instance of <see cref="ConsoleIOHandler"/> exists in the application.
        /// </remarks>
        /// <seealso cref="Utils.ConsoleIOHandler"/>
        public class ConsoleIOManager : ConsoleIOHandler
        {
            private static readonly ConsoleIOManager instance = new ConsoleIOManager();

            public static ConsoleIOManager Instance
            {
                get { return instance; }
            }

            /// <summary>
            /// logs notice message to console, replacing each format item in <paramref name="format"/> with 
            /// the string representation of the corresponding object in <paramref name="args"/>.
            /// </summary>
            /// <param name="flushOutputBuffer"></param>
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
            public void LogNoticeFormat(bool flushOutputBuffer, string format, params object[] args)
            {
                string message = string.Format(format, args);
                LogNotice(message, flushOutputBuffer);
            }

            /// <summary>
            /// logs a notice <paramref name="message"/> to console.
            /// </summary>
            /// <seealso cref="ConsoleIOHandler.QueueOutput(string, bool)"/> 
            /// <param name="message"></param>
            /// <param name="flushOutputBuffer"></param>
            /// <exception cref="ObjectDisposedException"><see cref="ConsoleIOHandler.QueueOutput(string, bool)"/>
            /// </exception>
            public void LogNotice(string message, bool flushOutputBuffer = false)
            {
                string outputString = getLogMessage(message);
                base.QueueOutput(outputString, flushOutputBuffer);
            }

            /// <summary>
            /// logs an error message to console.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="ConsoleIOHandler.QueueOutput(string, bool)"/>
            /// </exception>
            /// <seealso cref="ConsoleIOHandler.QueueOutput(string, bool)"/> 
            /// <param name="message"></param>
            /// <param name="flushOutputBuffer"></param>
            public void LogError(string message, bool flushOutputBuffer = false)
            {
                string outputString = getLogMessage(message);
                base.QueueOutput(outputString, flushOutputBuffer);
            }

            /// <summary>
            /// logs error message to console, replacing each format item in <paramref name="format"/> with 
            /// the string representation of the corresponding object in <paramref name="args"/>.
            /// </summary>
            /// <param name="flushOutputBuffer"></param>
            /// <param name="format">a composite format string</param>
            /// <param name="args">object array containing zero or more objects to format</param>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="string.Format(string, object[])"/>
            /// </exception>
            /// <exception cref="FormatException">
            /// <seealso cref="string.Format(string, object[])"/>
            /// </exception>
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="LogError(string, bool)"/>
            /// </exception> 
            public void LogErrorFormat(bool flushOutputBuffer, string format, params object[] args)
            {
                string message = string.Format(format, args);
                LogError(message, flushOutputBuffer);
            }

            /// <summary>
            /// prints <paramref name="data"/> to console.
            /// </summary>
            /// <seealso cref="ConsoleIOHandler.QueueOutput(string, bool)"/> 
            /// <param name="message"></param>
            /// <param name="flushOutputBuffer"></param>
            /// <exception cref="ObjectDisposedException">
            /// <see cref="ConsoleIOHandler.QueueOutput(string, bool)"/>
            /// </exception> 
            /// <exception cref="ObjectDisposedException">
            /// <seealso cref="QueueOutput(string,bool)"/>
            /// </exception>
            public void PrintData(string data, bool flushOutputBuffer = false)
            {
                string outputString = data + Environment.NewLine;
                base.QueueOutput(outputString, flushOutputBuffer);
            }

            /// <summary>
            /// prints a new line char sequence to Console.
            /// </summary>
            /// <seealso cref="ConsoleIOHandler.QueueOutput(string, bool)"/> 
            /// <param name="flushOutputBuffer"></param>
            /// <exception cref="ObjectDisposedException">
            /// <see cref="ConsoleIOHandler.QueueOutput(string, bool)"/>
            /// </exception>
            public void PrintNewLine(bool flushOutputBuffer = false)
            {
                string outputString = Environment.NewLine;
                QueueOutput(outputString, flushOutputBuffer);
            }

            public void showPressAnyKeyToContinueDialog()
            {
                string dialogPromptMessage = "Press any key to continue ..";
                LogNotice(dialogPromptMessage);

                // wait for key press
                base.ReadKey();
            }

            /// <summary>
            /// synchroniously displays a confirmation dialog with <paramref name="promptMessage"/>, allowing user
            /// to choose either 'yes' or 'no', and returns user choice.
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
            public bool ShowConfirmationDialog(string promptMessage)
            {
                bool userChoice;

                // construct dialog display message
                string dialogPromptMessage = promptMessage + " (Y/N)";
                LogNotice(dialogPromptMessage);

                // read user input
                string userInput = base.ReadLine().ToLower();

                // keep requesting input so long as it's not valid
                while(userInput != "y" && userInput != "n")
                {
                    LogNotice("Invalid input, please select 'Y' or 'N'");
                    userInput = base.ReadLine().ToLower();
                }

                userChoice = userInput == "y" ? true : false;

                return userChoice;
            }

            /// <summary>
            /// returns a log message containing <paramref name="message"/>.
            /// </summary>
            /// <seealso cref="Utils.DateTimeUtils.GetLogMessage(string)"/>
            /// <param name="message"></param>
            /// <returns></returns>
            private string getLogMessage(string message)
            {
                return DateTimeUtils.GetLogMessage(message) + Environment.NewLine; ;
            }
        }
    }
}
