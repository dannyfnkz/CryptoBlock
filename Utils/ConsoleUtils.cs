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

            public static int CursorLeft
            {
                get { return Console.CursorLeft; }
                set { Console.CursorLeft = value; }
            }

            public static int CursorTop
            {
                get { return Console.CursorTop; }
                set { Console.CursorTop = value; }
            }

            // returns default(ConsoleKeyInfo) if no key is available
            /// <summary>
            /// returns the most recent <see cref="ConsoleKeyInfo"/>from console input buffer, if available.
            /// </summary>
            /// <remarks>
            /// <para>the read key is removed from the Console input buffer.</para>
            /// <para>implemented using <see cref=System.Console.KeyAvailable/></para>
            /// <para>
            /// <see cref="System.Console.ReadKey()"/>
            /// <see cref="System.Console.SetCursorPosition(int, int)"/>
            /// </para>
            /// </remarks>
            /// <param name="keyAvailable">set to true if console input buffer has an available key,
            /// else it is set to false.</param>
            /// <returns>
            /// if Console input buffer is not empty, most recent <see cref="ConsoleKeyInfo"/>
            /// else, <c>default(<see cref="System.ConsoleKeyInfo"/>)</c>
            /// </returns>
            public static ConsoleKeyInfo ReadKey(out bool keyAvailable)
            {
                ConsoleKeyInfo consoleKeyInfo;

                keyAvailable = Console.KeyAvailable;

                if (keyAvailable)
                {
                    // read key
                    consoleKeyInfo = Console.ReadKey(true);

                    // write key to console if it has a textual representation
                    if (HasTextualConsoleRepresentation(consoleKeyInfo)) 
                    {
                        Console.Write(consoleKeyInfo.KeyChar);
                    }
                }
                else // no key available in Console input buffer
                {
                    consoleKeyInfo = default(ConsoleKeyInfo);
                }

                return consoleKeyInfo;
            }

            /// <summary>
            /// clears the console line currently pointed to by the cursor.
            /// </summary>
            /// <seealso cref="System.Console.SetCursorPosition(int, int)"/>
            /// <seealso cref="System.Console.Write(string)"/>
            public static void ClearCurrentConsoleLine()
            {
                // save current cursor horizontal position
                int cursorLeft = Console.CursorLeft;

                // set cursor to beginning of line
                Console.SetCursorPosition(0, Console.CursorTop);

                // clear current line
                Console.Write(new string(' ', Console.WindowWidth));

                // restore cursor position
                Console.SetCursorPosition(cursorLeft, Console.CursorTop - 1);
            }

            public static void SetCursorPosition(int cursorLeft, int cursorTop)
            {
                Console.SetCursorPosition(cursorLeft, cursorTop);
            }

            public static void SetCursorToBeginningOfNextLine()
            {
                SetCursorPosition(0, CursorTop + 1);
            }

            public static void MoveCursorHorizontal(int moveAmountHorizontal)
            {
                SetCursorPosition(CursorLeft + moveAmountHorizontal, CursorTop);
            }

            public static void MoveCursorVertical(int moveAmountVertical)
            {
                SetCursorPosition(CursorLeft, CursorTop + moveAmountVertical);
            }

            public static void ClearCurrentConsoleLineAndWrite(string output, params object[] args)
            {
                ClearCurrentConsoleLine();
                PointCursorToBeginningOfLine();
                ConsoleWrite(output, args);           
            }

            public static bool HasTextualConsoleRepresentation(ConsoleKeyInfo consoleKeyInfo)
            {
                return consoleKeyInfo.Key != ConsoleKey.Escape
                    && consoleKeyInfo.Key != ConsoleKey.DownArrow
                    && consoleKeyInfo.Key != ConsoleKey.UpArrow
                    && consoleKeyInfo.Key != ConsoleKey.RightArrow
                    && consoleKeyInfo.Key != ConsoleKey.LeftArrow;
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

            public static void PointCursorToBeginningOfLine()
            {
                CursorLeft = 0;
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

            public static bool IsCursorPointingToEndOfLine()
            {
                return Console.CursorLeft == Console.WindowWidth - 1;
            }

            public static void ConsoleWrite(string output, params object[] args)
            {
                Console.Write(output, args);
            }

            public static void ConsoleWriteLine(string output, params object[] args)
            {
                Console.WriteLine(output, args);
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
                ConsoleWriteLine(logMessage);
            }
        }
    }
}