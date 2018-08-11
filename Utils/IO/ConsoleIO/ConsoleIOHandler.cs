using CryptoBlock.Utils.CollectionUtils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.ConsoleIO
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
            /// <summary>
            /// manages storing & browsing input history.
            /// recent input entries are added to the stack upto a limit of
            /// <see cref="RECENT_INPUT_ENTRY_STACK_CAPACITY"/>, after which the least recent entries are
            /// discarded in favour of newer ones.
            /// browsing is done by pressing the down arrow (displays newer entries)
            /// and up arrow (displays earlier entries)
            /// </summary>
            private class InputHistoryManager
            {
                // max number of input entries stack holds simultaneously
                private const int RECENT_INPUT_ENTRY_STACK_CAPACITY = 50;

                // holds most recent input entries
                // if capcity is reached, least recent entries are discarded in favour of newer ones
                private readonly IndexableStack<string> recentInputEntries =
                    new IndexableStack<string>(RECENT_INPUT_ENTRY_STACK_CAPACITY);

                // index of input entry which is currently selected - 0 is the most recent entry
                private int selectedEntryIndex;

                // user is currently browsing input history
                private bool browsingActive;

                private ConsoleIOHandler consoleIOHandler;

                public InputHistoryManager(ConsoleIOHandler consoleIOHandler)
                {
                    this.consoleIOHandler = consoleIOHandler;
                }

                /// <summary>
                /// max number of input entries which can be simultaneously stored in history. 
                /// </summary>
                public int Capacity
                {
                    get { return RECENT_INPUT_ENTRY_STACK_CAPACITY; }
                }

                /// <summary>
                /// adds <paramref name="inputEntry"/> to input history.
                /// if <see cref="Capacity"/> is reached, the least recent entry is removed to make room
                /// for <paramref name="inputEntry"/>.
                /// </summary>
                /// <param name="inputEntry">input entry to be inserted to input history</param>
                public void AddInputEntryToHistory(string inputEntry)
                {
                    // avoid adding consecutive duplicate entries to stack
                    bool addInputEntry =
                        recentInputEntries.Empty
                        || inputEntry != recentInputEntries.TopElement();

                    if(addInputEntry)
                    {
                        recentInputEntries.Push(inputEntry);
                    }                  
                }

                /// <summary>
                /// manages input history browsing based on user key press,
                /// encapsulated by <paramref name="consoleKeyInfo"/>.
                /// </summary>
                /// <remarks>
                /// <para>
                /// this method should be called each time user presses a key,
                /// in order for browsing mechanism to be responsive.
                /// </para>
                /// <para>
                /// if user selects a particular entry, either by hitting an end-of-input key ('Enter' by default)
                /// or by editing the entry, all entries up to (i.e less recent than) selected entry
                /// are removed from the history.
                /// </para>
                /// </remarks>
                /// <param name="consoleKeyInfo">user-pressed key</param>
                public void HandleInputHistoryBrowsing(ConsoleKeyInfo consoleKeyInfo)
                {
                    if (isBrowsingKey(consoleKeyInfo)) // pressed key was a browsing key
                    {
                        handleBrowsingKey(consoleKeyInfo);
                    }

                    // pressed key is a textual key (i.e it changes the input),
                    // and browsing procedure is active
                    else if (ConsoleIOUtils.IsTextualKey(consoleKeyInfo) && browsingActive)
                    {
                        // set browsing procedure state to inactive
                        browsingActive = false;

                        // pop original user input (from before browsing started)
                        recentInputEntries.Pop();

                        // reset selected entry index 
                        selectedEntryIndex = 0;
                    }
                }

                /// <summary>
                /// returns whether <paramref name="consoleKeyInfo"/> is used by the input entry browsing mechanism.
                /// </summary>
                /// <param name="consoleKeyInfo">user-pressed key</param>
                /// <returns></returns>
                private static bool isBrowsingKey(ConsoleKeyInfo consoleKeyInfo)
                {
                    return consoleKeyInfo.Key == ConsoleKey.DownArrow
                        || consoleKeyInfo.Key == ConsoleKey.UpArrow;
                }

                /// <summary>
                /// handles browsing key encapsulated by <paramref name="consoleKeyInfo"/>,
                /// as defined by the browsing mechanism:
                /// <see cref="ConsoleKey.UpArrow"/> displays less recent entries;
                /// <see cref="ConsoleKey.DownArrow"/> displays more recent entries.
                /// </summary>
                /// <param name="consoleKeyInfo"></param>
                private void handleBrowsingKey(ConsoleKeyInfo consoleKeyInfo)
                {
                    // amount to increment / decrement index of currently selected input entry
                    int selectedEntryIndexIncrementAmount = 0;

                    if (consoleKeyInfo.Key == ConsoleKey.UpArrow)
                    {
                        selectedEntryIndexIncrementAmount = 1;

                        // start of browsing procedure
                        if (!browsingActive)
                        {
                            // push current user input to stack so it can be retrieved later
                            string userInput = consoleIOHandler.GetInputBufferContent();
                            recentInputEntries.Push(userInput);

                            // set browsing procedure state to active
                            browsingActive = true;
                        }
                    }
                    else // consoleKeyInfo.Key == ConsoleKey.DownArrow
                    {
                        selectedEntryIndexIncrementAmount = -1;

                        // top of recent input entry stack reached (back to original user input)
                        if (browsingActive && selectedEntryIndex == 0)
                        {
                            // pop original user input from stack
                            string originalUserInput = recentInputEntries.Pop();

                            // set browsing procedure state to inactive
                            browsingActive = false;
                        }
                    }

                    // still within stack bounds, top or bottom not yet reached
                    if (recentInputEntries.HasElementAt(
                        selectedEntryIndex + selectedEntryIndexIncrementAmount))
                    {
                        // increment selected input entry index
                        selectedEntryIndex += selectedEntryIndexIncrementAmount;

                        // get selected entry from stack
                        string selectedInputEntry =
                            recentInputEntries.ElementAt(selectedEntryIndex);

                        // overwrite current console input line
                        ConsoleGraphicsHandler.OverwriteInputLine(selectedInputEntry);

                        // overwrite input buffer
                        consoleIOHandler.FlushInputBuffer();
                        consoleIOHandler.appendToInputBuffer(selectedInputEntry);
                    }
                }
            }

            // sleep time for input & output listen threads
            private const int INPUT_LISTEN_DELAY_TIME_MILLIS = 10;
            private const int OUTPUT_FLUSH_DELAY_TIME_MILLIS = 10;

            // input
            private StringBuilder inputBuffer = new StringBuilder();
            InputHistoryManager inputHistoryManager;

            // when true, input is registered
            // when false, input is ignored
            private bool registerInput = true;
            private bool consoleInputListenThreadRunning = true;

            // output
            private Queue<string> outputBuffer = new Queue<string>();

            // when true, output is registered
            // when false, output is ignored
            private bool registerOutput = true;
            private bool outputFlushThreadRunning = true;

            // when true, output is periodically flushed to Console
            private bool outputAutoFlush = true;

            // user recently requested an output flush
            private bool outputFlushRequested = false;

            // object was disposed
            private bool disposed = false;

            // raised after an end-of-input console key was registered
            public event Action<string> EndOfInputKeyRegistered;

            public ConsoleIOHandler()
            {
                // init recent input browser
                this.inputHistoryManager = new InputHistoryManager(this);

                // start listening to console input
                startConsoleInputListenThread();

                // start output flush thread
                startOutputFlushThread();
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
            /// sets the register state of input and output.
            /// </summary>
            /// <exception cref="ObjectDisposedException"><see cref="assertNotDisposed()"/></exception>
            /// <seealso cref="RegisterInput"/>
            /// <seealso cref="RegisterOutput"/>
            /// <param name="registerInput"></param>
            /// <param name="registerOutput"></param>
            public void SetIORegisterState(bool registerInput, bool registerOutput)
            {
                assertNotDisposed();

                RegisterInput = registerInput;
                RegisterOutput = registerOutput;
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

            [MethodImpl(MethodImplOptions.Synchronized)]
            public int GetInputBufferCount()
            {
                assertNotDisposed();
                return inputBuffer.Length;
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
            /// inserts <paramref name="str"/> to the output buffer if <see cref="registerOutput"/> is true.
            /// if <paramref name="requestFlush"/> is true, requests output buffer flush after inserting 
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

            // synchronious
            /// input is not registered by the input listen thread,
            /// and is therefore not appended to the input buffer.
            public char ReadKey()
            {
                ConsoleKeyInfo pressedConsoleKey = Console.ReadKey(true);
    
                return pressedConsoleKey.KeyChar;
            }

            /// <summary>
            /// synchroniously reads user console input until 'Enter' key is pressed, then returns said input.
            /// </summary>
            /// <remarks>
            /// input is not registered by the input listen thread,
            /// and is therefore not appended to the input buffer.
            /// </remarks>
            /// <returns>
            /// user input read from console
            /// </returns>
            public string ReadLine()
            {
                assertNotDisposed();

                // force output flush before reading line if output auto flush is enabled
                if (outputAutoFlush)
                {
                    ForceOutputBufferFlush();
                }

                // save original state of output auto flush
                bool outputAutoFlushOriginalState = OutputAutoFlush;

                // switch output auto flush off
                OutputAutoFlush = false;

                // read user input (synchroniously)
                string userInput = Console.ReadLine();

                // restore output auto flush original state
                OutputAutoFlush = outputAutoFlushOriginalState;

                return userInput;
            }

            /// <summary>
            /// writes output string <paramref name="str"/> to Console.
            /// </summary>
            /// <remarks>
            /// default implementation writes <paramref name="output"/> string as-is to Console.
            /// </remarks>
            /// <seealso cref="System.Console.Write(string)"/>
            /// <param name="output"></param>
            protected virtual void WriteOutput(string output)
            {
                ConsoleGraphicsHandler.HandleOutput(output);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            private void appendToInputBuffer(string input)
            {
                inputBuffer.Append(input);
            }

            private void appendToInputBuffer(char input)
            {
                appendToInputBuffer(input.ToString());
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            private void insertToInputBuffer(int index, string input)
            {
                inputBuffer.Insert(index, input);
            }

            private void insertToInputBuffer(int index, char input)
            {
                insertToInputBuffer(index, input.ToString());
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            private void RemoveFromInputBuffer(int startIndex, int length)
            {
                inputBuffer.Remove(startIndex, length);
            }

            /// <summary>
            /// starts input listen thread, which registers Console key presses.
            /// </summary>
            /// <seealso cref="ReadKeyIfAvailable()"/>
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void startConsoleInputListenThread()
            {
                Task consoleInputListenTask = new Task(new Action(listenToConsoleInput));
                consoleInputListenTask.Start();
            }

            /// <summary>
            /// continuously reads a user-pressed key from console input buffer(if available),
            /// then sleeps for a specified timeout period.
            /// runs as long as <see cref="consoleInputListenThreadRunning"/> is set to true.
            /// </summary>
            /// <seealso cref="readKeyIfAvailable"/>
            /// <seealso cref="System.Threading.Thread.Sleep(int)"/>
            private void listenToConsoleInput()
            {
                while (consoleInputListenThreadRunning)
                {
                    readKeyIfAvailable();
                    try
                    {
                        Task.Delay(INPUT_LISTEN_DELAY_TIME_MILLIS).Wait();
                    }
                    catch(AggregateException) // thrown by Task.Delay(int).wait()
                    {

                    }
                }
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

                            try
                            {
                                Task.Delay(OUTPUT_FLUSH_DELAY_TIME_MILLIS).Wait();
                            }
                            catch (AggregateException) // thrown by Task.Delay(int).wait()
                            {

                            }
                        }
                    }
                });

                outputListenTask.Start();
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

            /// <summary>
            /// if <see cref="registerInput"/> is true, reads a single key from Console input buffer (if available)
            /// and stores its char representation in input buffer.
            /// else, clears the Console input buffer.
            /// </summary>
            /// <remarks>
            /// asynchronous operation (returns immediately if no key is available in Console input buffer).
            /// </remarks>
            /// <seealso cref="ConsoleIOUtils.ReadKey(out bool)"/>
            /// <seealso cref="handleInputConsoleKey(ConsoleKeyInfo)"/>
            /// <seealso cref="handleEndOfInputKey()"/>
            /// <seealso cref="handleInputBackspaceKey()"/>
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void readKeyIfAvailable()
            {
                ConsoleKeyInfo consoleKeyInfo = ConsoleIOUtils.ReadKey(out bool keyAvailable);

                // key available in console input buffer
                if (keyAvailable)
                {           
                    if (registerInput)
                    {                        
                        handleInputConsoleKey(consoleKeyInfo);
                        ConsoleGraphicsHandler.HandleInputKey(consoleKeyInfo);
                    }
                    else
                    {
                        // discard Console input
                        ConsoleIOUtils.ClearConsoleInputBuffer();
                    }          
                }                    
            }

            /// <summary>
            /// handles a user-pressed console key, encapsulated by <paramref name="consoleKeyInfo"/>.
            /// if <paramref name="consoleKeyInfo"/> represents a textual key it is appended to input buffer,
            /// else it is treated accordingly.
            /// </summary>
            /// <seealso cref="AppendToInputBuffer(char)"/>
            /// <seealso cref="handleInputEnterKey"/>
            /// <seealso cref="handleInputBackspaceKey"/>
            /// <seealso cref="handleInputLeftArrowKey"/>
            /// <seealso cref="handleInputRightArrowKey"/>
            /// <seealso cref="handleInputBackspaceKey"/>
            /// <seealso cref="AppendToInputBuffer"/>
            /// <param name="consoleKeyInfo">a user-pressed console key</param>
            private void handleInputConsoleKey(ConsoleKeyInfo consoleKeyInfo)
            {
                // handle input browsing
                this.inputHistoryManager.HandleInputHistoryBrowsing(consoleKeyInfo);

                if (consoleKeyInfo.Key == ConsoleKey.Enter) // key represents enter
                {
                    handleInputEnterKey();
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Backspace) // key represents backspace
                {
                    handleInputBackspaceKey();
                }
                // only keys with textual representation are inserted to input buffer
                else if (ConsoleIOUtils.IsTextualKey(consoleKeyInfo))
                {
                    if (consoleKeyInfo.Key == ConsoleKey.Backspace)
                    {
                        handleInputBackspaceKey();
                    }
                    else
                    {
                        if(GetInputBufferCount() < ConsoleIOUtils.WindowWidth - 1) // input line is not full
                        {
                            // insert to input buffer at current cursor position
                            insertToInputBuffer(ConsoleIOUtils.CursorLeft, consoleKeyInfo.KeyChar);
                        }                       
                    }
                }
            }

            /// <summary>
            /// raises <see cref="EndOfInputKeyRegistered"/> event with the string representation
            /// of the input buffer as parameter.
            /// </summary>
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void onEndOfInputKeyRegistered()
            {
                string inputLine = inputBuffer.ToString();

                // raise EndOfInputKeyRegistered event
                if (EndOfInputKeyRegistered != null)
                {                   
                    EndOfInputKeyRegistered.Invoke(inputLine);
                }

                // add input entry to recent input browser
                this.inputHistoryManager.AddInputEntryToHistory(inputLine);
            }

            /// <summary>
            /// handles end-of-input key press.
            /// </summary>
            /// <seealso cref="onEndOfInputKeyRegistered()"/>
            private void handleInputEnterKey()
            {
                // raise EndOfInputKeyRegistered event
                onEndOfInputKeyRegistered();
            }

            /// <summary>
            /// handles backspace key press.
            /// if input buffer is not empty, deletes the most recently inserted character
            /// from <see cref="inputBuffer"/>.
            /// </summary>
            private void handleInputBackspaceKey()
            {
                // remove character to left of cursor from input buffer if not empty
                if (ConsoleIOUtils.CursorLeft > 0)
                {
                    RemoveFromInputBuffer(ConsoleIOUtils.CursorLeft - 1, 1);
                }
            }

            // flush is done on the calling thread
            /// <summary>
            /// forces flush on output buffer. flush is done synchronously.
            /// </summary>
            /// <seealso cref="flushOutputBuffer"/>
            protected void ForceOutputBufferFlush()
            {
                flushOutputBuffer();
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
                // it is assumed that user input spans at most one line in console.
                bool restoreInputToConsoleFlag = false;

                // cursor doesn't point to beginning of line, user might be in the middle of typing
                if (!ConsoleIOUtils.IsCursorPointingToBeginningOfLine())
                {
                    // clear the current console line (holding user input)
                    ConsoleGraphicsHandler.ClearCurrentLine();

                    // restore user input after flush
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
            [MethodImpl(MethodImplOptions.Synchronized)]
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