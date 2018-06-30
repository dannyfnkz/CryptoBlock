using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        public class ConsoleUtils
        {
            // returns default(ConsoleKeyInfo) if no key is available
            public static ConsoleKeyInfo ReadKey()
            {
                ConsoleKeyInfo consoleKeyInfo;

                if (Console.KeyAvailable)
                {
                    // read key
                    consoleKeyInfo = Console.ReadKey();

                    // handle backspace key press
                    if(consoleKeyInfo.Key == ConsoleKey.Backspace)
                    {
                        // remove character at current cursor position
                        Console.Write(' ');

                        // move cursor one character backwards
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    }
                    // handle enter key press
                    else if(consoleKeyInfo.Key == ConsoleKey.Enter)
                    {
                        // move cursor to beginning of next line
                        Console.SetCursorPosition(0, Console.CursorTop + 1);
                    }
                    else if (consoleKeyInfo.KeyChar != '\0') // handle textual key press
                    {
                        
                    }
                }
                else
                {
                    consoleKeyInfo = default(ConsoleKeyInfo);
                }

                return consoleKeyInfo;
            }

            public static void ClearConsoleInputBuffer()
            {
                while(Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                }
            }

            public static void LogLine(string value)
            {
                string logMessage = DateTimeUtils.GetLogMessage(value);
                Console.WriteLine(logMessage);
            }

            public static bool IsCurrentCursorLineEmpty()
            {
                return Console.CursorLeft == 0;
            }
        }
    }
}