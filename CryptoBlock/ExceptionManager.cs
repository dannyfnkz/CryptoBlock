using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    internal class ExceptionManager
    {
        private const string LOG_FILE_PATH = @"error_log.txt";
        private static readonly string LOG_FILE_HEADER = string.Format(
            @"CryptoBlock Error Log File{0}{1}",
            Environment.NewLine,
            Environment.NewLine);

        public static readonly ExceptionManager Instance = new ExceptionManager();

        internal void PrintGenericServerDataFetchExceptionMessage()
        {
            ConsoleUtils.LogLine("An error occurred while trying to fetch data from server."
                + " Refer to error log file for detailed information.");
        }

        internal void LogException(Exception exception)
        {
            bool newFile = false;

            if(!File.Exists(LOG_FILE_PATH))
            {
                newFile = true;

                // create a new log file
                using (StreamWriter streamWriter = File.CreateText(LOG_FILE_PATH))
                {
                    streamWriter.Write(LOG_FILE_HEADER);
                }
            }

            // append exception message to log file
            using (StreamWriter streamWriter = File.AppendText(LOG_FILE_PATH))
            {
                // add padding between two entries
                if (!newFile)
                {
                    streamWriter.WriteLine();
                }

                string exceptionMessageHeader = string.Format(
                    "[{0}]{1}",
                    DateTimeUtils.GetCurrentDateTimeString(),
                    Environment.NewLine);

                streamWriter.Write(exceptionMessageHeader);

                string exceptionMessage = ExceptionUtils.GetExceptionMessageString(exception);

                streamWriter.Write(exceptionMessage);

                streamWriter.WriteLine();
            }
        }
    }
}
