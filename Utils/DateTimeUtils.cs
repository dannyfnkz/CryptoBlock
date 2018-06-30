using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        /// <summary>
        /// contains utility methods for dealing with date/time information.
        /// </summary>
        public static class DateTimeUtils
        {
            /// <summary>
            /// returns the current Unix timestamp (number of seconds elapsed since 01/01/1970, 00:00:00 UTC).
            /// </summary>
            /// <returns>
            /// current Unix timestamp.
            /// </returns>
            public static long GetUnixTimestamp()
            {
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            /// <summary>
            /// returns a string representing the current date and time in the format
            /// yyyy-MM-dd HH:mm:ss (e.g 1993-01-20 04:21:30).
            /// </summary>
            /// <returns>
            /// string representing the current date and time in the format yyyy-MM-dd HH:mm:ss.
            /// </returns>
            public static string GetCurrentDateTimeString()
            {
                return DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss");
            }

            /// <summary>
            /// returns a string containing <paramref name="message"/>preceded by
            /// the current datetime string representation (<see cref="GetCurrentDateTimeString()"/>).
            /// </summary>
            /// <param name="message"></param>
            /// <returns>
            /// string containing specified message preceded by the current datetime.
            /// </returns>
            public static string GetLogMessage(string message)
            {
                string dateTimeHeader = GetCurrentDateTimeString();

                string logMessage = string.Format("[{0}]: {1}", dateTimeHeader, message);

                return logMessage;
            }
        }
    }
}

