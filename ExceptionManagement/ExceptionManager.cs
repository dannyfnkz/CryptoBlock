using CryptoBlock.IOManagement;
using CryptoBlock.Utils;
using System;
using System.IO;
using System.Text;

namespace CryptoBlock
{
    namespace ExceptionManagement
    {
        /// <summary>
        /// manages exception handling.
        /// </summary>
        public class ExceptionManager
        {
            /// <summary>
            /// thrown if an exception occurred while trying to write to error log file.
            /// </summary>
            public class ErrorLogFileWriteException : Exception
            {
                public ErrorLogFileWriteException(Exception innerExcepiton)
                    : base (formatExceptionMessage(), innerExcepiton)
                {

                }

                private static string formatExceptionMessage()
                {
                    return "An error occurred while trying to write to error log file.";
                }
            }

            private const string ERROR_LOG_FILE_PATH = "error_log.txt";

            // positioned at top of error log file
            private static readonly string ERROR_LOG_FILE_HEADER = string.Format(
                @"CryptoBlock Error Log File{0}",
                Environment.NewLine);

            private static readonly ExceptionManager instance = new ExceptionManager();

            public static ExceptionManager Instance
            {
                get { return instance; }
            }

            /// <summary>
            /// logs message refering user to error log file for more information.
            /// </summary>
            public void ConsoleLogReferToErrorLogFileMessage()
            {
                ConsoleIOManager.Instance.LogError("Refer to error log file for more information.");
            }

            /// <summary>
            /// logs <paramref name="exception"/> to error log file.
            /// </summary>
            /// <param name="exception"></param>
            public void LogToErrorFile(Exception exception)
            {
                logToErrorFile(exception, false);
            }

            /// <summary>
            /// <para>
            /// logs <paramref name="exception"/> to error log file.
            /// </para>
            /// <para>
            /// if an exception is thrown while trying to write to log file,
            /// and <paramref name="previousLogAttemptFailed"/> is false, attempt another write
            /// (this time setting <paramref name="previousLogAttemptFailed"/> to true, so that in total
            /// only two write attempts are made.)
            /// </para>
            /// </summary>
            /// <param name="exception"></param>
            /// <param name="previousLogAttemptFailed"></param>
            private void logToErrorFile(Exception exception, bool previousLogAttemptFailed)
            {
                try
                {
                    // construct exception log
                    StringBuilder stringBuilder = new StringBuilder();

                    if (!FileIOManager.Instance.FileExists(ERROR_LOG_FILE_PATH))
                    {
                        // write error log file header
                        stringBuilder.Append(ERROR_LOG_FILE_HEADER);
                    }

                    // add padding between two entries / between header and first entry
                    stringBuilder.Append(Environment.NewLine);

                    string exceptionMessageHeader = string.Format(
                        "[{0}]{1}",
                        DateTimeUtils.GetCurrentDateTimeString(),
                        Environment.NewLine);

                    stringBuilder.Append(exceptionMessageHeader);

                    string exceptionMessage = ExceptionUtils.GetExceptionMessageString(exception);

                    stringBuilder.Append(exceptionMessage);

                    stringBuilder.Append(Environment.NewLine);

                    // write log to error log file
                    FileIOManager.Instance.AppendTextToFile(ERROR_LOG_FILE_PATH, stringBuilder.ToString());
                }
                catch (Exception ex) // error occurred while trying to write to error log file
                {
                    if(!previousLogAttemptFailed) // first failed attempt to write to log file
                    {
                        logToErrorFile(new ErrorLogFileWriteException(ex), true);
                    }                 
                }
            }
        }
    }
}
