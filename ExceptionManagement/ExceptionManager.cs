using CryptoBlock.IOManagement;
using CryptoBlock.Utils;
using System;
using System.IO;
using System.Text;

namespace CryptoBlock
{
    namespace ExceptionManagement
    {
        public class ExceptionManager
        {
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

            private const string ERROR_LOG_FILE_NAME = "error_log";
            private static readonly string ERROR_LOG_FILE_HEADER = string.Format(
                @"CryptoBlock Error Log File{0}",
                Environment.NewLine);

            public static readonly ExceptionManager Instance = new ExceptionManager();

            public void ConsoleLogReferToErrorLogFileMessage()
            {
                ConsoleIOManager.Instance.LogError("Refer to error log file for more information.");
            }

            public void LogException(Exception exception)
            {
                try
                {
                    // construct exception log
                    StringBuilder stringBuilder = new StringBuilder();

                    if (!FileIOManager.Instance.ErrorLogFileExists(ERROR_LOG_FILE_NAME))
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
                    FileIOManager.Instance.AppendTextToErrorLogFile(ERROR_LOG_FILE_NAME, stringBuilder.ToString());
                }
                catch (Exception ex) // error occurred while trying to write to error log file
                {
                    LogException(new ErrorLogFileWriteException(ex));
                }
            }
        }
    }
}
