using CryptoBlock.Utils;
using System;

namespace CryptoBlock
{
    /// <summary>
    /// manages Console input / output operations.
    /// </summary>
    /// <remarks>
    /// only one instance of <see cref="ConsoleIOHandler"/> exists in the application.
    /// </remarks>
    /// <seealso cref="Utils.ConsoleIOHandler"/>
    internal class ConsoleIOManager : ConsoleIOHandler
    {
        private static readonly ConsoleIOManager instance = new ConsoleIOManager();

        internal static ConsoleIOManager Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// logs a notice message to console.
        /// </summary>
        /// <exception cref="ObjectDisposedException"><see cref="ConsoleIOHandler.QueueOutput(string, bool)"/>
        /// </exception>
        /// <seealso cref="ConsoleIOHandler.QueueOutput(string, bool)"/> 
        /// <param name="message"></param>
        /// <param name="flushOutputBuffer"></param>
        internal void LogNotice(string message, bool flushOutputBuffer = false)
        {
            string outputString = getLogMessage(message);
            QueueOutput(outputString, flushOutputBuffer);
        }

        /// <summary>
        /// logs an error message to console.
        /// </summary>
        /// <exception cref="ObjectDisposedException"><see cref="ConsoleIOHandler.QueueOutput(string, bool)"/>
        /// </exception>
        /// <seealso cref="ConsoleIOHandler.QueueOutput(string, bool)"/> 
        /// <param name="message"></param>
        /// <param name="flushOutputBuffer"></param>
        internal void LogError(string message, bool flushOutputBuffer = false)
        {
            string outputString = getLogMessage(message);
            QueueOutput(outputString, flushOutputBuffer);
        }

        /// <summary>
        /// logs a data message to console.
        /// </summary>
        /// <exception cref="ObjectDisposedException"><see cref="ConsoleIOHandler.QueueOutput(string, bool)"/>
        /// </exception>
        /// <seealso cref="ConsoleIOHandler.QueueOutput(string, bool)"/> 
        /// <param name="message"></param>
        /// <param name="flushOutputBuffer"></param>
        internal void LogData(string message, bool flushOutputBuffer = false)
        {
            string outputString = message + Environment.NewLine;
            QueueOutput(outputString, flushOutputBuffer);
        }

        /// <summary>
        /// prints a new line char sequence to Console.
        /// </summary>
        /// <exception cref="ObjectDisposedException"><see cref="ConsoleIOHandler.QueueOutput(string, bool)"/>
        /// </exception>
        /// <seealso cref="ConsoleIOHandler.QueueOutput(string, bool)"/> 
        /// <param name="flushOutputBuffer"></param>
        internal void PrintNewLine(bool flushOutputBuffer = false)
        {
            string outputString = Environment.NewLine;
            QueueOutput(outputString, flushOutputBuffer);
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