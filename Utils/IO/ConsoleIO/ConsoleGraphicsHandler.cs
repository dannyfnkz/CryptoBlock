using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.ConsoleIO
    {
        /// <summary>
        /// handles the console graphics.
        /// </summary>
        internal static class ConsoleGraphicsHandler
        {
            // holds contents of input line
            private static StringBuilder inputLineBuffer = new StringBuilder(ConsoleIOUtils.WindowWidth);

            /// <summary>
            /// handles key press encapsulated in <paramref name="consoleKeyInfo"/>,
            /// and dislays its textual representation to console, if one exists.
            /// </summary>
            /// <param name="consoleKeyInfo"></param>
            internal static void HandleInputKey(ConsoleKeyInfo consoleKeyInfo)
            {
                if (consoleKeyInfo.Key == ConsoleKey.Enter) // key represents enter
                {
                    handleInputEnterKey();
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Backspace) // key represents backspace
                {
                    handleInputBackspaceKey();
                }
                else if (consoleKeyInfo.Key == ConsoleKey.LeftArrow) // key represents left-arrow
                {
                    handleInputLeftArrowKey();
                }
                else if (consoleKeyInfo.Key == ConsoleKey.RightArrow) // key represents right-arrow
                {
                    handleInputRightArrowKey();
                }
                // key has textual representation
                else if (
                    ConsoleIOUtils.IsTextualKey(consoleKeyInfo) 
                    && ConsoleIOUtils.CursorLeft < ConsoleIOUtils.WindowWidth - 1)
                {
                    // insert key char representation to line buffer
                    insertToLineBufferAtCurrentPosition(consoleKeyInfo.KeyChar.ToString());

                    if(ConsoleIOUtils.CursorLeft == inputLineBuffer.Length - 1)  // append to end of line
                    {
                        Console.Write(consoleKeyInfo.KeyChar.ToString());
                    }
                    else // insert within line
                    {
                        // rewrite current line so that insert within line buffer is visible
                        rewriteInputLineToCosole();

                        // move 1 character to the right
                        ConsoleIOUtils.MoveCursorHorizontal(1);
                    }
                }
            }

            /// <summary>
            /// handles <paramref name="output"/> and displays it to console.
            /// </summary>
            /// <param name="output"></param>
            internal static void HandleOutput(string output)
            {
                ConsoleIOUtils.ConsoleWrite(output);
                setLineBufferToOutputLastLine(output);
            }

            /// <summary>
            /// overwrites the content of the input line with <paramref name="newInput"/>.
            /// </summary>
            /// <param name="newInput"></param>
            internal static void OverwriteInputLine(string newInput)
            {
                ClearConsoleInputLine();
                HandleOutput(newInput);
            }

            /// <summary>
            /// clears the console input line.
            /// </summary>
            internal static void ClearConsoleInputLine()
            {
                ConsoleIOUtils.ClearCurrentConsoleLine();
                ConsoleIOUtils.PointCursorToBeginningOfLine();

                // clear input line buffer
                inputLineBuffer.Clear();
            }

            /// <summary>
            /// sets the contents of <see cref="inputLineBuffer"/> to be the last line of 
            /// <paramref name="output"/>.
            /// </summary>
            /// <param name="output"></param>
            private static void setLineBufferToOutputLastLine(string output)
            {
                int indexOfLastNewline = output.LastIndexOf(Environment.NewLine);

                if (indexOfLastNewline >= 0) // newline found
                {
                    inputLineBuffer.Clear();

                    // append last line in output to lineBuffer
                    string lastLine = output.Substring(indexOfLastNewline + Environment.NewLine.Length);
                    inputLineBuffer.Append(lastLine);
                }
                else // newline not found
                {
                    inputLineBuffer.Append(output);
                }
            }

            /// <summary>
            /// handles end-of-input key press.
            /// </summary>
            private static void handleInputEnterKey()
            {
                // move cursor to beginning of next line
                ConsoleIOUtils.SetCursorToBeginningOfNextLine();
            }

            /// <summary>
            /// handles backspace key press. removes character to the left of cursor from console.
            /// </summary>
            private static void handleInputBackspaceKey()
            {
                if(ConsoleIOUtils.CursorLeft > 0) // not at beginning of line
                {
                    // move cursor one character backwards
                    ConsoleIOUtils.MoveCursorHorizontal(-1);

                    // remove current character from line buffer
                    removeCharacterFromLineBufferAtCurrentPosition();

                    // rewrite console input line without removed character
                    rewriteInputLineToCosole();
                }
            }

            /// <summary>
            /// rewrites the contents of <see cref="inputLineBuffer"/> to the console input line.
            /// </summary>
            private static void rewriteInputLineToCosole()
            {
                // save current line and cursor position
                string currentLine = inputLineBuffer.ToString();
                int cursorLeft = ConsoleIOUtils.CursorLeft;

                // clear line
                ClearConsoleInputLine();

                // rewrite line and restore cursor position
                HandleOutput(currentLine);
                ConsoleIOUtils.CursorLeft = cursorLeft;
            }

            /// <summary>
            /// handles left arrow key user key press.
            /// </summary>
            private static void handleInputLeftArrowKey()
            {
                // if not at beginning of line, move cursor one character backwards
                if (!ConsoleIOUtils.IsCursorPointingToBeginningOfLine())
                {
                    ConsoleIOUtils.MoveCursorHorizontal(-1);
                }
            }

            /// <summary>
            /// handles right arrow key user key press.
            /// </summary>
            private static void handleInputRightArrowKey()
            {
                // if cursor is within line bounds, move cursor one character rightwards
                if(ConsoleIOUtils.CursorLeft < inputLineBuffer.Length)
                {
                    ++ConsoleIOUtils.CursorLeft;
                }
            }

            /// <summary>
            /// removes from <see cref="inputLineBuffer"/> the character located at the position
            /// cursor is currently pointing to.
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException">
            /// thrown if cursor is pointing to start of input line
            /// </exception>
            private static void removeCharacterFromLineBufferAtCurrentPosition()
            {
                inputLineBuffer.Remove(ConsoleIOUtils.CursorLeft, 1);
            }

            /// <summary>
            /// inserts <paramref name="insertString"/> into <see cref="inputLineBuffer"/> at the position
            /// cursor is currently pointing to.
            /// </summary>
            /// <param name="insertString"></param>
            private static void insertToLineBufferAtCurrentPosition(string insertString)
            {
                inputLineBuffer.Insert(ConsoleIOUtils.CursorLeft, insertString);
            }
        }
    }
}