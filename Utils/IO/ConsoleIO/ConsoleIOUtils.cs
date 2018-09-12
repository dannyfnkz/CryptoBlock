using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.ConsoleIO
    {
        /// <summary>
        /// contains methods which provide additional utility for <see cref="System.Console"/>.
        /// </summary>
        public class ConsoleIOUtils
        {
            /// <summary>
            /// the horizontal offset, in characters, of the console cursor from the beginning of the line
            /// it is pointing to.
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <seealso cref="Console.CursorLeft"/>
            /// </exception>
            /// <exception cref="IOException">
            /// <seealso cref="Console.CursorLeft"/>
            /// </exception>
            public static int CursorLeft
            {
                get { return Console.CursorLeft; }
                set { Console.CursorLeft = value; }
            }

            /// <summary>
            /// the vertical offset, in lines, of the console cursor from the top of the console window.
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <seealso cref="Console.CursorTop"/>
            /// </exception>
            /// <exception cref="IOException">
            /// <seealso cref="Console.CursorTop"/>
            /// </exception>
            public static int CursorTop
            {
                get { return Console.CursorTop; }
                set { Console.CursorTop = value; }
            }

            public static int WindowWidth
            {
                get { return Console.WindowWidth; }
                set { Console.WindowWidth = value; }
            }

            // returns default(ConsoleKeyInfo) if no key is available
            /// <summary>
            /// returns the most recent <see cref="ConsoleKeyInfo"/>from console input buffer, if available.
            /// </summary>
            /// <remarks>
            /// <para>the read key is removed from the Console input buffer.</para>
            /// <para>implemented using <see cref="System.Console.KeyAvailable"/></para>
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
            /// <param name="cursorTop"></param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <seealso cref="Console.SetCursorPosition(int, int)"/>
            /// </exception>
            /// <exception cref="IOException">
            /// <seealso cref="Console.SetCursorPosition(int, int)"/>
            /// </exception>
            public static ConsoleKeyInfo ReadKey(out bool keyAvailable)
            {
                keyAvailable = Console.KeyAvailable;

                // read key from console input buffer if available
                const bool interceptKey = true; // don't write obtained key to console
                ConsoleKeyInfo consoleKeyInfo = keyAvailable ? Console.ReadKey(interceptKey): default(ConsoleKeyInfo);

                return consoleKeyInfo;
            }

            /// <summary>
            /// clears the console line currently pointed to by the cursor.
            /// </summary>
            /// <seealso cref="System.Console.SetCursorPosition(int, int)"/>
            /// <seealso cref="ConsoleWrite(string)"/>
            public static void ClearCurrentConsoleLine()
            {
                // save current cursor horizontal position
                int cursorLeft = Console.CursorLeft;

                // set cursor to beginning of line
                Console.SetCursorPosition(0, CursorTop);

                // clear current line
                ConsoleWrite(new string(' ', WindowWidth));

                // restore cursor position
                Console.SetCursorPosition(cursorLeft, Console.CursorTop - 1);
            }

            /// <summary>
            /// sets the console cursor position to (x,y) = (<paramref name="cursorLeft"/>, <paramref name="cursorTop"/>)
            /// </summary>
            /// <param name="cursorLeft"></param>
            /// <param name="cursorTop"></param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <seealso cref="Console.SetCursorPosition(int, int)"/>
            /// </exception>
            /// <exception cref="IOException">
            /// <seealso cref="Console.SetCursorPosition(int, int)"/>
            /// </exception>
            public static void SetCursorPosition(int cursorLeft, int cursorTop)
            {
                Console.SetCursorPosition(cursorLeft, cursorTop);
            }

            /// <summary>
            /// sets the console cursor position to beginning of next line.
            /// </summary>
            /// <seealso cref="Console.SetCursorPosition(int, int)"/>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <seealso cref="SetCursorPosition(int, int)"/>
            /// </exception>
            /// <exception cref="IOException">
            /// <seealso cref="SetCursorPosition(int, int)"/>
            /// </exception>
            public static void SetCursorToBeginningOfNextLine()
            {
                SetCursorPosition(0, CursorTop + 1);
            }

            /// <summary>
            /// moves console cursor horizontally, <paramref name="moveAmountHorizontal"/> characters to the right.
            /// if <paramref name="moveAmountHorizontal"/> is negative, moves cursor to the left.
            /// </summary>
            /// <param name="moveAmountHorizontal"></param>
            /// <seealso cref="Console.SetCursorPosition(int, int)"/>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <seealso cref="SetCursorPosition(int, int)"/>
            /// </exception>
            /// <exception cref="IOException">
            /// <seealso cref="SetCursorPosition(int, int)"/>
            /// </exception>
            public static void MoveCursorHorizontal(int moveAmountHorizontal)
            {
                SetCursorPosition(CursorLeft + moveAmountHorizontal, CursorTop);
            }

            /// <summary>
            /// moves console cursor vertically, <paramref name="moveAmountHorizontal"/> lines upwards.
            /// if <paramref name="moveAmountVertical"/> is negative, moves cursor downwards.
            /// </summary>
            /// <param name="moveAmountVertical"></param>
            /// <seealso cref="Console.SetCursorPosition(int, int)"/>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <seealso cref="SetCursorPosition(int, int)"/>
            /// </exception>
            /// <exception cref="IOException">
            /// <seealso cref="SetCursorPosition(int, int)"/>
            /// </exception>
            public static void MoveCursorVertical(int moveAmountVertical)
            {
                SetCursorPosition(CursorLeft, CursorTop + moveAmountVertical);
            }

            /// <summary>
            /// clears the line console cursor is currently pointing to,
            /// and writes <paramref name="output"/> to same line, formatting <paramref name="output"/>
            /// according to <paramref name="args"/> if it is a composite format string.
            /// </summary>
            /// <seealso cref="ConsoleWrite(string, params object[])"/>
            /// <param name="output">plain output string or a composite format string</param>
            /// <param name="args">an object array which contains zero or more objects to format</param>
            public static void ClearCurrentConsoleLineAndWrite(string output, params object[] args)
            {
                ClearCurrentConsoleLine();
                PointCursorToBeginningOfLine();
                ConsoleWrite(output, args);           
            }

            /// <summary>
            /// points the console cursor to the beginning of the line it is currently pointing to.
            /// </summary>
            public static void PointCursorToBeginningOfLine()
            {
                CursorLeft = 0;
            }

            /// <summary>
            /// returns true if <paramref name="consoleKeyInfo"/> has a textual console representation 
            /// </summary>
            /// <remarks>
            /// note keys checked are only those which are caught by <see cref="ReadKey(out bool)"/>.
            /// </remarks>
            /// <param name="consoleKeyInfo">
            /// a <see cref="System.ConsoleKeyInfo"/> caught by <see cref="ReadKey(out bool)"/>
            /// </param>
            /// <returns>
            /// true if <paramref name="consoleKeyInfo"/> has a textual console representation, else false
            /// </returns>
            public static bool IsTextualKey(ConsoleKeyInfo consoleKeyInfo)
            {
                return consoleKeyInfo.Key != ConsoleKey.Escape
                    && consoleKeyInfo.Key != ConsoleKey.DownArrow
                    && consoleKeyInfo.Key != ConsoleKey.UpArrow
                    && consoleKeyInfo.Key != ConsoleKey.RightArrow
                    && consoleKeyInfo.Key != ConsoleKey.LeftArrow;
            }

            /// <summary>
            /// clears the Console input buffer.
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

            /// <summary>
            /// returns whether if console cursor is pointing to the end of a line.
            /// </summary>
            /// <returns>
            ///  true if console cursor is pointing to the end of a line,
            ///  else false
            /// </returns>
            public static bool IsCursorPointingToEndOfLine()
            {
                return Console.CursorLeft == Console.WindowWidth - 1;
            }

            /// <summary>
            /// writes <paramref name="output"/> to the line cursor is currently pointing to.
            /// if <paramref name="output"/> is a composite format string, formats string according to
            /// <paramref name="args"/>.
            /// <seealso cref="System.Console.Write(string, params object[])"/>
            /// </summary>
            /// <param name="output">plain output string or a composite format string</param>
            /// <param name="args">an object array which contains zero or more objects to format</param>
            public static void ConsoleWrite(string output, params object[] args)
            {
                string formattedOutput = string.Format(output, args);
                Console.Write(formattedOutput);
            }

            /// <summary>
            /// writes <paramref name="ch"/> to console.
            /// </summary>
            /// <seealso cref="ConsoleWrite(String)"/>
            /// <param name="ch"></param>
            public static void ConsoleWrite(char ch)
            {
                ConsoleWrite(ch.ToString());
            }

            /// <summary>
            /// replaces character at <paramref name="horizontalCursorPosition"/> of line cursor is
            /// currently pointing to with <paramref name="replacementCharacter"/>.
            /// </summary>
            /// <param name="horizontalCursorPosition"></param>
            /// <param name="replacementCharacter"></param>
            public static void ReplaceCharacter(int horizontalCursorPosition, char replacementCharacter)
            {
                // move cursor to speicfied horizontal position
                CursorLeft = horizontalCursorPosition;

                // replace current character
                ConsoleWrite(replacementCharacter.ToString());

                // revert forward moved caused by ConsoleWrite
                MoveCursorHorizontal(-1);
            }

            public static void ReplaceCurrentCharacter(char replacementCharacter)
            {
                ReplaceCharacter(CursorLeft, replacementCharacter);
            }

            /// <summary>
            /// writes <paramref name="output"/> to the line cursor is currently pointing to,
            /// formatting <paramref name="output"/> according to <paramref name="args"/>
            /// if it is a composite format string.
            /// </summary>
            /// <seealso cref="ConsoleWrite(string, params object[])"/>
            /// <param name="output">plain output string or a composite format string</param>
            /// <param name="args">an object array which contains zero or more objects to format</param>
            public static void ConsoleWriteLine(string output, params object[] args)
            {
                string outputWithNewline = output + Environment.NewLine;
                ConsoleWrite(outputWithNewline, args);
            }

            /// <summary>
            /// prints log message (<see cref="DateTimeUtils.GetLogMessage(value)"/> of
            /// <paramref name="value"/>in a new line to Console.
            /// </summary>
            /// <seealso cref="System.Console"/>
            /// <param name="value"></param>
            public static void LogLine(string value)
            {
                string logMessage = DateTimeUtils.FormatLogMessage(value);
                ConsoleWriteLine(logMessage);
            }
        }
    }
}