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
        public class ConsoleIOHandler : IDisposable
        {
            private const int LISTEN_THREAD_SLEEP_TIME_MILLIS = 10;

            private List<ConsoleKey> endOfInputConsoleKeys = new List<ConsoleKey>();

            private StringBuilder inputBuffer = new StringBuilder();
            private bool inputEnabled = true;
            private bool consoleInputListenThreadRunning = true;

            private Queue<string> outputBuffer = new Queue<string>();
            private bool outputEnabled = true;
            private bool outputListenThreadRunning = true;
            private bool outputAutoFlush = true;
            private bool outputFlushRequested = false;

            private bool disposed = false;

            public const ConsoleKey DEFAULT_INPUT_FLUSH_CONSOLE_KEY = ConsoleKey.Enter;

            public event Action<string> EndOfInputKeyRead;

            public ConsoleIOHandler()
            {
                endOfInputConsoleKeys.Add(DEFAULT_INPUT_FLUSH_CONSOLE_KEY);

                // start listening to console input
                startConsoleInputListenThread();

                // start listening to output
                startOutputListenThread();
            }

            ~ConsoleIOHandler()
            {
                Dispose();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void Dispose()
            {
                assertNotDisposed();

                consoleInputListenThreadRunning = false;
                outputListenThreadRunning = false;
                disposed = true;
            }

            public List<ConsoleKey> EndOfInputConsoleKeys
            {
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    assertNotDisposed();

                    return endOfInputConsoleKeys;
                }
            }

            public bool OutputAutoFlush
            {
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    assertNotDisposed();

                    return outputAutoFlush;
                }

                [MethodImpl(MethodImplOptions.Synchronized)]
                set
                {
                    assertNotDisposed();

                    outputAutoFlush = value;
                }
            }

            public bool InputEnabled
            {
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    assertNotDisposed();

                    return inputEnabled;
                }

                [MethodImpl(MethodImplOptions.Synchronized)]
                set
                {
                    assertNotDisposed();

                    inputEnabled = value;
                }
            }

            public bool OutputEnabled
            {
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    assertNotDisposed();

                    return outputEnabled;
                }

                [MethodImpl(MethodImplOptions.Synchronized)]
                set
                {
                    assertNotDisposed();

                    outputEnabled = value;
                }
            }

            public bool InputAvailable
            {
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    assertNotDisposed();

                    return inputBuffer.Length > 0;
                }
            }

            public bool OutputAvailable
            {
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    assertNotDisposed();

                    return outputBuffer.Count > 0;
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void AddEndOfInputConsoleKey(ConsoleKey consoleKey)
            {
                assertNotDisposed();

                endOfInputConsoleKeys.Add(consoleKey);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void AddEndOfInputConsoleKeyRange(IEnumerable<ConsoleKey> consoleKeys)
            {
                assertNotDisposed();

                endOfInputConsoleKeys.AddRange(consoleKeys);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public bool RemoveEndOfInputConsoleKey(ConsoleKey consoleKey)
            {
                assertNotDisposed();

                return endOfInputConsoleKeys.Remove(consoleKey);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void ClearEndOfInputConsoleKey()
            {
                assertNotDisposed();

                endOfInputConsoleKeys.Clear();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void SetIOEnabledState(bool inputEnabled, bool outputEnabled)
            {
                assertNotDisposed();

                this.inputEnabled = inputEnabled;
                this.outputEnabled = outputEnabled;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public string GetInputBufferContent()
            {
                assertNotDisposed();

                return inputBuffer.ToString();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public string FlushInputBuffer()
            {
                assertNotDisposed();

                string inputBufferContent = GetInputBufferContent();

                ClearInputBuffer();

                return inputBufferContent;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void ClearInputBuffer()
            {
                assertNotDisposed();

                inputBuffer.Clear();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void ClearOutputBuffer()
            {
                assertNotDisposed();

                outputBuffer.Clear();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void RequestOutputBufferFlush()
            {
                assertNotDisposed();

                outputFlushRequested = true;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void QueueOutput(string str, bool requestFlush = false)
            {
                assertNotDisposed();

                if (!outputEnabled)
                {
                    return;
                }

                outputBuffer.Enqueue(str);

                if (requestFlush)
                {
                    outputFlushRequested = true;
                }
            }

            protected virtual void WriteOutput(string str)
            {
                 Console.Write(str);
            }

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

            [MethodImpl(MethodImplOptions.Synchronized)]
            private void startOutputListenThread()
            {
                Task outputListenTask = new Task(() =>
                {
                    while (outputListenThreadRunning)
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
            
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void clearInputFromConsole()
            {
                // move cursor one character backwards
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }

            private void restoreInputToConsole()
            {
                if (inputBuffer.Length > 0)
                {
                    WriteOutput(inputBuffer.ToString());
                }
            }

            // asynchronous (returns immediately if no key is available)
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void ReadKeyIfAvailable()
            {
                if (!inputEnabled)
                {
                    ConsoleUtils.ClearConsoleInputBuffer();
                    return;
                }

                ConsoleKeyInfo consoleKeyInfo = ConsoleUtils.ReadKey();

                // user pressed a key
                if (consoleKeyInfo != default(ConsoleKeyInfo))
                {
                    if (endOfInputConsoleKeys.Contains(consoleKeyInfo.Key))
                    {
                        handleEndOfInputKey();
                    }
                    else if (consoleKeyInfo.KeyChar != '\0') // text character
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

            [MethodImpl(MethodImplOptions.Synchronized)]
            private void handleEndOfInputKey()
            {
                // notify listeners when end of input key is input
                if (EndOfInputKeyRead != null)
                {
                    string inputLine = inputBuffer.ToString();
                    EndOfInputKeyRead.Invoke(inputLine);
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            private void handleInputBackspaceKey()
            {
                // remove character from input buffer if not empty
                if (inputBuffer.Length > 0)
                {
                    inputBuffer.Remove(inputBuffer.Length - 1, 1);
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            private void flushOutputBuffer()
            {
                bool restoreInputToConsoleFlag = false;

                if (!ConsoleUtils.IsCurrentCursorLineEmpty())
                {
                    clearInputFromConsole();
                    restoreInputToConsoleFlag = true;
                }

                while (outputBuffer.Count > 0)
                {
                    string outPutEntry = outputBuffer.Dequeue();
                    WriteOutput(outPutEntry);
                }

                if (restoreInputToConsoleFlag)
                {
                    restoreInputToConsole();
                }
            }

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
