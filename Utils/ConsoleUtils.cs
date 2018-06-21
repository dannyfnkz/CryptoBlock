using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        public class ConsoleUtils
        {
            public static void LogLine(string value)
            {
                string dateTimeHeader = DateTimeUtils.GetCurrentDateTimeString();

                string line = string.Format("[{0}]: {1}", dateTimeHeader, value);

                Console.WriteLine(line);
            }
        }
    }
}