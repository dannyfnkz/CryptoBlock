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
        /// <summary>
        /// contains methods which provide additional utility for <see cref="System.Console"/>.
        /// </summary>
        public class ConsoleUtils
        {
            // returns default(ConsoleKeyInfo) if no key is available
            /// <summary>
            /// returns the most recent <see cref="ConsoleKeyInfo"/>from Console input buffer, if available.
            /// </summary>
            /// <remarks>
            /// <para></para>the read key is removed from the Console input buffer.</para>
            /// <para></para>implemented using <see cref=System.Console.KeyAvailable/>,
            /// <see cref="System.Console.ReadKey()"/>,<see cref="System.Console.SetCursorPosition(int, int)"/>.</para>
            /// </remarks>
            /// <returns>
            /// if Console input buffer is not empty, most recent <see cref="ConsoleKeyInfo"/>
            /// else, <c>default(<see cref="System.ConsoleKeyInfo"/>)</c>
            /// </returns>
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
                }
                else // no key available in Console input buffer
                {
                    consoleKeyInfo = default(ConsoleKeyInfo);
                }

                return consoleKeyInfo;
            }

            /// <summary>
            /// empties the Console input buffer.
            /// <seealso cref="System.Console"/>
            /// </summary>
            public static void ClearConsoleInputBuffer()
            {
                while(Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                }
            }

            /// <summary>
            /// prints log message (<see cref="DateTimeUtils.GetLogMessage(value)"/> of
            /// <paramref name="value"/>in a new line to Console.
            /// </summary>
            /// <seealso cref="System.Console"/>
            /// <param name="value"></param>
            public static void LogLine(string value)
            {
                string logMessage = DateTimeUtils.GetLogMessage(value);
                Console.WriteLine(logMessage);
            }

            /// <summary>
            /// returns whether the cursor is pointing to the beginning of a line.
            /// </summary>
            /// <returns>
            /// true if cursor is pointing to the beginning of a line,
            /// else false.
            /// </returns>
            public static bool IsCursorPointingToBeginningOfLine()
            {
                return Console.CursorLeft == 0;
            }
        }
    }
}