using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    internal class ConsoleIOManager : ConsoleIOHandler
    {
        private static readonly ConsoleIOManager instance = new ConsoleIOManager();

        internal static ConsoleIOManager Instance
        {
            get { return instance; }
        }

        internal void LogNotice(string message, bool flushOutputBuffer = false)
        {
            string outputString = getLogMessage(message);
            QueueOutput(outputString, flushOutputBuffer);
        }

        internal void LogError(string message, bool flushOutputBuffer = false)
        {
            string outputString = getLogMessage(message);
            QueueOutput(outputString, flushOutputBuffer);
        }

        internal void LogData(string message, bool flushOutputBuffer = false)
        {
            string outputString = message + Environment.NewLine;
            QueueOutput(outputString, flushOutputBuffer);
        }

        internal void PrintNewLine(bool flushOutputBuffer = false)
        {
            string outputString = Environment.NewLine;
            QueueOutput(outputString, flushOutputBuffer);
        }

        private string getLogMessage(string message)
        {
            return DateTimeUtils.GetLogMessage(message) + Environment.NewLine; ;
        }
    }
}
