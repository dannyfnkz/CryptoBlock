using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        /// <summary>
        /// handles Console input / output.
        /// allows accessing input / output buffers and on-demand / auto flushing.
        /// </summary>
        /// <remarks>
        /// starts a separate thread for input interception and an additional thread for output flushing,
        /// which can be (irrevocably, for this object) stopped by calling <see cref="Dispose()"/>.
        /// </remarks>
        public class ConsoleIOHandler : IDisposable
        {
            // sleep time for input & output listen threads
            private const int LISTEN_THREAD_SLEEP_TIME_MILLIS = 10;

            // when registered, cause EndOfInputKeyRead event to fire
            private List<ConsoleKey> endOfInputConsoleKeys = new List<ConsoleKey>();

            // input
            private StringBuilder inputBuffer = new StringBuilder();

            // console input should be registered (when false, console input is ignored)
            private bool registerInput = true;
            private bool consoleInputListenThreadRunning = true;

            // output
            private Queue<string> outputBuffer = new Queue<string>();

            // output should be registered (when false, output is ignored) 
            private bool registerOutput = true;
            private bool outputFlushThreadRunning = true;

            // when true, output is periodically flushed to Console
            private bool outputAutoFlush = true;

            // user recently requested an output flush
            private bool outputFlushRequested = false;

            // object was disposed
            private bool disposed = false;

            public const ConsoleKey DEFAULT_END_OF_INPUT_CONSOLE_KEY = ConsoleKey.Enter;

            // fired after an end-of-input console key was registered
            public event Action<string> EndOfInputKeyRegistered;

            public ConsoleIOHandler()
            {
                endOfInputConsoleKeys.Add(DEFAULT_END_OF_INPUT_CONSOLE_KEY);

                // start listening to console input
                startConsoleInputListenThread();

                // start output flush thread
                startOutputFlushThread();
            }

            ~ConsoleIOHandler()
            {
                Dispose();
            }

            /// <summary>
            /// list of end-of-input <see cref="ConsoleKey"/>s.
            /// when a key in the list is intercepted by Console input 
            /// and subsequently registered in ConsoleIOHandler,
            /// EndOfInputKeyRegistered event is raised. 
            /// </summary>
            public List<ConsoleKey> EndOfInputConsoleKeys
            {
                /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    assertNotDisposed();

                    return endOfInputConsoleKeys;
                }
            }

            /// <summary>
            /// when true, output is periodically flushed to Console.
            /// </summary>
            /// <remarks>
            /// default value is true.
            /// </remarks>
            public bool OutputAutoFlush
            {
                /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    assertNotDisposed();

                    return outputAutoFlush;
                }

                /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
                [MethodImpl(MethodImplOptions.Synchronized)]
                set
                {
                    assertNotDisposed();

                    outputAutoFlush = value;
                }
            }
 
            /// <summary>
            /// when true, Console input is registered and stored in input buffer.
            /// when false, Console input is ignored.
            /// </summary>
            /// <remarks>
            /// default value is true.
            /// </remarks>
            public bool RegisterInput
            {
                /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    assertNotDisposed();

                    return registerInput;
                }

                /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
                [MethodImpl(MethodImplOptions.Synchronized)]
                set
                {
                    assertNotDisposed();

                    registerInput = value;
                }
            }

            /// <summary>
            /// when true, output is registered and stored in output buffer.
            /// when false, output is ignored.
            /// </summary>
            /// <remarks>
            /// default value is true.
            /// </remarks>
            public bool RegisterOutput
            {
                /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    assertNotDisposed();

                    return registerOutput;
                }

                /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
                [MethodImpl(MethodImplOptions.Synchronized)]
                set
                {
                    assertNotDisposed();

                    registerOutput = value;
                }
            }

            /// <summary>
            /// input is available in input buffer (input buffer is not empty).
            /// </summary>
            public bool InputAvailable
            {
                /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    assertNotDisposed();

                    return inputBuffer.Length > 0;
                }
            }

            /// <summary>
            /// output is available in output buffer (output buffer is not empty).
            /// </summary>
            public bool OutputAvailable
            {
                /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    assertNotDisposed();

                    return outputBuffer.Count > 0;
                }
            }

            /// <summary>
            /// irrevocably stops Console input listen and output flush threads for this object.
            /// sets <see cref="disposed"/> to true.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            /// <seealso cref="System.IDisposable"/>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void Dispose()
            {
                assertNotDisposed();

                // stop run of input and output listen threads
                consoleInputListenThreadRunning = false;
                outputFlushThreadRunning = false;

                disposed = true;
            }

            /// <summary>
            /// adds <paramref name="consoleKey"/>to list of end-of-input console keys.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            /// <seealso cref="EndOfInputConsoleKeys"/>
            /// <param name="consoleKey"></param>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void AddEndOfInputConsoleKey(ConsoleKey consoleKey)
            {
                assertNotDisposed();

                endOfInputConsoleKeys.Add(consoleKey);
            }

            /// <summary>
            /// adds all ConsoleKeys in <paramref name="consoleKeys"/>to end-of-input <see cref="ConsoleKey"/>list.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            /// <seealso cref="EndOfInputConsoleKeys"/>
            /// <param name="consoleKeys"></param>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void AddEndOfInputConsoleKeyRange(IEnumerable<ConsoleKey> consoleKeys)
            {
                assertNotDisposed();

                endOfInputConsoleKeys.AddRange(consoleKeys);
            }

            /// <summary>
            /// removes <paramref name="consoleKey"/>from end-of-input <see cref="ConsoleKey"/>list.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            /// <seealso cref="EndOfInputConsoleKeys"/>
            /// <param name="consoleKey"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public bool RemoveEndOfInputConsoleKey(ConsoleKey consoleKey)
            {
                assertNotDisposed();

                return endOfInputConsoleKeys.Remove(consoleKey);
            }

            /// <summary>
            /// clears end-of-input <see cref="ConsoleKey"/> list.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            /// <seealso cref="EndOfInputConsoleKeys"/> 
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void ClearEndOfInputConsoleKey()
            {
                assertNotDisposed();

                endOfInputConsoleKeys.Clear();
            }

            /// <summary>
            /// sets the register state of input and output.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            /// <seealso cref="RegisterInput"/>
            /// <seealso cref="RegisterOutput"/>
            /// <param name="registerInput"></param>
            /// <param name="registerOutput"></param>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void SetIORegisterState(bool registerInput, bool registerOutput)
            {
                assertNotDisposed();

                this.registerInput = registerInput;
                this.registerOutput = registerOutput;
            }

            /// <summary>
            /// returns the content of the input buffer as a string.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            /// <returns>
            /// input buffer content as string.
            /// </returns>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public string GetInputBufferContent()
            {
                assertNotDisposed();

                return inputBuffer.ToString();
            }

            /// <summary>
            /// clears the input buffer and returns the content of the input buffer as a string.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            /// <returns>
            /// input buffer content as string.
            /// </returns>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public string FlushInputBuffer()
            {
                assertNotDisposed();

                string inputBufferContent = GetInputBufferContent();

                ClearInputBuffer();

                return inputBufferContent;
            }

            /// <summary>
            /// clears the input buffer.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void ClearInputBuffer()
            {
                assertNotDisposed();

                inputBuffer.Clear();
            }

            /// <summary>
            /// clears the output buffer.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void ClearOutputBuffer()
            {
                assertNotDisposed();

                outputBuffer.Clear();
            }

            /// <summary>
            /// requests that the output buffer be flushed to Console.
            /// </summary>
            /// <remarks>
            /// output buffer is flushed during the next run of the output listen thread. 
            /// </remarks>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            /// <seealso cref="flushOutputBuffer()"/>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void RequestOutputBufferFlush()
            {
                assertNotDisposed();

                outputFlushRequested = true;
            }

            /// <summary>
            /// inserts <paramref name="str"/>to the output buffer if <see cref="registerOutput"/>is true.
            /// if <paramref name="requestFlush"/>is true, requests output buffer flush after inserting
            /// <paramref name="str"/>.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            /// <seealso cref="RequestOutputBufferFlush()"/>
            /// <param name="str"></param>
            /// <param name="requestFlush"></param>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void QueueOutput(string str, bool requestFlush = false)
            {
                assertNotDisposed();

                if (!registerOutput)
                {
                    return;
                }

                outputBuffer.Enqueue(str);

                if (requestFlush)
                {
                    outputFlushRequested = true;
                }
            }

            /// <summary>
            /// writes output string <paramref name="str"/> to Console.
            /// </summary>
            /// <remarks>
            /// default implementation writes output string as-is to Console.
            /// override to modify output string before write to Console.
            /// </remarks>
            /// <seealso cref="System.Console.Write(string)"/>
            /// <param name="str"></param>
            protected virtual void WriteOutput(string str)
            {
                 Console.Write(str);
            }

            /// <summary>
            /// starts input listen thread, which registers Console key presses.
            /// </summary>
            /// <seealso cref="ReadKeyIfAvailable()"/>
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void startConsoleInputListenThread()
            {
                Task consoleInputListenTask = new Task(() =>
                {
                    while (consoleInputListenThreadRunning)
                    {
                        ReadKeyIfAvailable();
                        Thread.Sleep(LISTEN_THREAD_SLEEP_TIME_MILLIS);
                    }
                });
                consoleInputListenTask.Start();
            }

            /// <summary>
            /// starts output flush thread, which periodically flushes the output buffer
            /// if <see cref="outputAutoFlush"/>or <see cref="outputFlushRequested"/>are set to true.
            /// </summary>
            /// <seealso cref="flushOutputBuffer()"/>
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void startOutputFlushThread()
            {
                Task outputListenTask = new Task(() =>
                {
                    while (outputFlushThreadRunning)
                    {
                        if(OutputAvailable && (outputAutoFlush || outputFlushRequested))
                        {
                            flushOutputBuffer();
                            outputFlushRequested = false;

                            Thread.Sleep(LISTEN_THREAD_SLEEP_TIME_MILLIS);
                        }
                    }
                });
                outputListenTask.Start();
            }
            
            /// <summary>
            /// clears the console line currently pointed to by the cursor.
            /// </summary>
            /// <seealso cref="System.Console.SetCursorPosition(int, int)"/>
            /// <seealso cref="System.Console.Write(string)"/>
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void clearCurrentConsoleLine()
            {
                // move cursor one character backwards
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }

            /// <summary>
            /// writes the current content of the input buffer to Console.
            /// </summary>
            /// <seealso cref="WriteOutput(string)"/>
            private void writeInputToConsole()
            {
                if (inputBuffer.Length > 0)
                {
                    WriteOutput(inputBuffer.ToString());
                }
            }

            // 
            /// <summary>
            /// if <see cref="registerInput"/> is true, reads a single key from Console input buffer (if available)
            /// and stores its char representation in input buffer.
            /// else, clears the Console input buffer.
            /// </summary>
            /// <remarks>
            /// asynchronous operation (returns immediately if no key is available in Console input buffer).
            /// </remarks>
            /// <seealso cref="Utils.ConsoleUtils.ClearConsoleInputBuffer()"/>
            /// <seealso cref="handleEndOfInputKey()"/>
            /// <seealso cref="handleInputBackspaceKey()"/>
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void ReadKeyIfAvailable()
            {
                if (!registerInput)
                {
                    // discarded Console input
                    ConsoleUtils.ClearConsoleInputBuffer();
                    return;
                }

                ConsoleKeyInfo consoleKeyInfo = ConsoleUtils.ReadKey();

                // key available in Console input buffer
                if (consoleKeyInfo != default(ConsoleKeyInfo))
                {
                    // key represents end-of-input
                    if (endOfInputConsoleKeys.Contains(consoleKeyInfo.Key))
                    {
                        handleEndOfInputKey();
                    }
                    // only keys with char representation are inserted to input buffer
                    else if (consoleKeyInfo.KeyChar != '\0') 
                    {
                        if (consoleKeyInfo.Key == ConsoleKey.Backspace)
                        {
                            handleInputBackspaceKey();
                        }
                        else
                        {
                            // append to input buffer
                            inputBuffer.Append(consoleKeyInfo.KeyChar);
                        }
                    }
                }
            }

            /// <summary>
            /// handles end-of-input key press.
            /// </summary>
            /// <seealso cref="onEndOfInputKeyRegistered()"/>
            private void handleEndOfInputKey()
            {
                onEndOfInputKeyRegistered();
            }

            /// <summary>
            /// raises <see cref="EndOfInputKeyRegistered"/> event with the string representation
            /// of the input buffer as parameter.
            /// </summary>
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void onEndOfInputKeyRegistered()
            {
                // notify listeners when end of input key is input
                if (EndOfInputKeyRegistered != null)
                {
                    string inputLine = inputBuffer.ToString();
                    EndOfInputKeyRegistered.Invoke(inputLine);
                }
            }

            /// <summary>
            /// handles backspace key press.
            /// if input buffer is not empty, deletes the most recently inserted character
            /// from <see cref="inputBuffer"/>.
            /// </summary>
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void handleInputBackspaceKey()
            {
                // remove character from input buffer if not empty
                if (inputBuffer.Length > 0)
                {
                    inputBuffer.Remove(inputBuffer.Length - 1, 1);
                }
            }

            /// <summary>
            /// flushes the output buffer to Console.
            /// </summary>
            /// <remarks>
            /// this implementation assumes user input spans at most one line in Console. 
            /// </remarks>
            /// <seealso cref="WriteOutput(string)"/>
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void flushOutputBuffer()
            {
                // save the line currently pointed to by cursor,
                // remove it from Console and write it back after the output buffer flush.
                // this is done in order to avoid deletion of user input from console,
                // in case user was in the middle of typing.
                // it is assumed that user input spans at most one line in Console.
                bool restoreInputToConsoleFlag = false;

                if (!ConsoleUtils.IsCursorPointingToBeginningOfLine())
                {
                    clearCurrentConsoleLine();
                    restoreInputToConsoleFlag = true;
                }

                // write all entries in output buffer to Console
                // in insert order.
                while (outputBuffer.Count > 0)
                {
                    string outPutEntry = outputBuffer.Dequeue();
                    WriteOutput(outPutEntry);
                }

                // restore input
                if (restoreInputToConsoleFlag)
                {
                    writeInputToConsole();
                }
            }

            /// <summary>
            /// asserts that object has not been disposed (and is therefore not usable).
            /// </summary>
            /// <exception cref="ObjectDisposedException">thrown if object has been disposed.</exception>
            private void assertNotDisposed()
            {
                if(disposed)
                {
                    throw new ObjectDisposedException("ConsoleIOHandler");
                }
            }
        }
    } 
}
