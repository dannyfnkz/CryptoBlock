using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.ConsoleIO
    {
        internal static class ConsoleGraphicsHandler
        {
            private static StringBuilder inputLineBuffer = new StringBuilder(ConsoleIOUtils.WindowWidth);

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
                else if (ConsoleIOUtils.IsTextualKey(consoleKeyInfo))
                {
                    // insert key char representation to line buffer
                    insertToLineBufferAtCurrentPosition(consoleKeyInfo.KeyChar.ToString());

                    if(ConsoleIOUtils.CursorLeft == inputLineBuffer.Length - 1)  // insert at end of line
                    {
                        Console.Write(consoleKeyInfo.KeyChar.ToString());
                    }
                    else // insert within line
                    {
                        // rewrite current line so that insert within line buffer is visible
                        rewriteCurrentLine();

                        // move 1 character to the right
                        ConsoleIOUtils.MoveCursorHorizontal(1);
                    }
                }
            }

            internal static void HandleOutput(string output)
            {
                ConsoleIOUtils.ConsoleWrite(output);

                //if (ConsoleIOUtils.CursorLeft == lineBuffer.Length)
                //{
                //    ConsoleIOUtils.ConsoleWrite(output);
                //}
                //else
                //{
                //    insertToLineBufferAtCurrentPosition(output);
                //    rewriteCurrentLine();
                //    lineBuffer.Clear();
                //}

                setLineBufferToOutputLastLine(output);
            }

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

            internal static void ClearCurrentLine()
            {
                ConsoleIOUtils.ClearCurrentConsoleLine();
                ConsoleIOUtils.PointCursorToBeginningOfLine();

                // clear line buffer
                inputLineBuffer.Clear();
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
                    removeFromLineBufferAtCurrentPosition();

                    // rewrite current line without removed character
                    rewriteCurrentLine();
                }
            }

            private static void rewriteCurrentLine()
            {
                // save current line and cursor position
                string currentLine = inputLineBuffer.ToString();
                int cursorLeft = ConsoleIOUtils.CursorLeft;

                // clear line
                ClearCurrentLine();

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

            private static void removeFromLineBufferAtCurrentPosition()
            {
                inputLineBuffer.Remove(ConsoleIOUtils.CursorLeft, 1);
            }

            private static void insertToLineBufferAtCurrentPosition(string insertString)
            {
                inputLineBuffer.Insert(ConsoleIOUtils.CursorLeft, insertString);
            }
        }
    }
}