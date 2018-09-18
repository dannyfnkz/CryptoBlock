using CryptoBlock.CommandHandling.Arguments;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using System;
using System.Collections.Generic;

namespace CryptoBlock
{
    namespace CommandHandling
    {
        /// <summary>
        /// represents a single executable command.
        /// </summary>
        public abstract class Command
        {
            ///// <summary>
            ///// thrown if user gave a wrong number of arguments for command. 
            ///// </summary>
            //public class WrongNumberOfArgumentsException : CommandExecutionException
            //{
            //    public WrongNumberOfArgumentsException(int minNumberOfArguments, int maxNumberOfArguments)
            //        : base(formatExceptionMessage(minNumberOfArguments, maxNumberOfArguments))
            //    {

            //    }

            //    private static string formatExceptionMessage(int minNumberOfArguments, int maxNumberOfArguments)
            //    {
            //        return string.Format("Wrong number of arguments: should be between {0} and {1}.",
            //            minNumberOfArguments,
            //            maxNumberOfArguments);
            //    }
            //}

            private readonly string prefix;
            
            private bool executed;

            protected readonly List<ICommandArgumentConstraint> commandArgumentConstraintList
                = new List<ICommandArgumentConstraint>();

            public Command(string prefix)
            {
                this.prefix = prefix;
                //this.minNumberOfArguments = minNumberOfArguments;
                //this.maxNumberOfArguments = maxNumberOfArguments;
            }

            /// <summary>
            /// unique prefix determines what command the user requested.
            /// </summary>
            public string Prefix
            {
                get { return prefix; }
            }

            public bool Executed
            {
                get { return executed; }
                protected set { executed = value; }
            }

            ///// <summary>
            ///// minimum number of arguments allowed for command.
            ///// </summary>
            //public int MinNumberOfArguments
            //{
            //    get { return minNumberOfArguments; }
            //}

            ///// <summary>
            ///// maximum number of arguments allowed for command.
            ///// </summary>
            //public int MaxNumberOfArguments
            //{
            //    get { return maxNumberOfArguments; }
            //}

            public void Handle(string[] commandArguments)
            {
                bool commandArgumentsValid = checkCommandArgumentConstraints(commandArguments);

                if(commandArgumentsValid)
                {
                    this.executed = Execute(commandArguments);
                }
            }

            protected static string FormatPrefix(
                string baseCommandPrefix,
                string inheritingCommandPrefix)
            {
                return string.Format("{0} {1}", baseCommandPrefix, inheritingCommandPrefix);
            }

            /// <summary>
            /// executes command with given <paramref name="commandArguments"/>.
            /// returns whether command was executed successfully.
            /// </summary>
            /// <param name="commandArguments"></param>
            /// <returns>true if command was executed successfully,,
            /// else false
            /// </returns>
            protected abstract bool Execute(string[] commandArguments);

            protected static void PrintCommandData(string message, bool flushOutputBuffer = false)
            {
                ConsoleIOManager.Instance.PrintData(
                    message,
                    ConsoleIOManager.eOutputReportType.CommandExecution,
                    flushOutputBuffer);
            }

            protected static void PrintCommandDataFormat(
                bool flushOutputBuffer,
                string format,
                params object[] args)
            {
                ConsoleIOManager.Instance.PrintDataFormat(
                    flushOutputBuffer,
                    ConsoleIOManager.eOutputReportType.CommandExecution,
                    format,
                    args);
            }

            protected static void LogCommandNotice(string message, bool flushOutputBuffer = false)
            {
                ConsoleIOManager.Instance.LogNotice(
                    message, 
                    ConsoleIOManager.eOutputReportType.CommandExecution,
                    flushOutputBuffer);
            }

            protected static void LogCommandNoticeFormat(
                bool flushOutputBuffer,
                string format,
                params object[] args)
            {
                ConsoleIOManager.Instance.LogNoticeFormat(
                    flushOutputBuffer,
                    ConsoleIOManager.eOutputReportType.CommandExecution,
                    format,
                    args);
            }

            protected static void LogCommandError(string message, bool flushOutputBuffer = false)
            {
                ConsoleIOManager.Instance.LogError(
                    message,
                    ConsoleIOManager.eOutputReportType.CommandExecution,
                    flushOutputBuffer);
            }

            protected static void LogCommandErrorFormat(
                bool flushOutputBuffer,
                string format,
                params object[] args)
            {
                ConsoleIOManager.Instance.LogErrorFormat(
                    flushOutputBuffer,
                    ConsoleIOManager.eOutputReportType.CommandExecution,
                    format,
                    args);
            }

            protected static void LogCommandReferToErrorLogFileMessage(bool flushOutputBuffer = false)
            {
                ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage(
                    ConsoleIOManager.eOutputReportType.CommandExecution, 
                    flushOutputBuffer);
            }

            //        public abstract void UndoCommand();

            private bool checkCommandArgumentConstraints(string[] commandArgumentArray)
            {
                foreach(ICommandArgumentConstraint commandArgumentConstrint in this.commandArgumentConstraintList)
                {
                    bool commandArgumentArrayValid = commandArgumentConstrint.IsValid(commandArgumentArray);

                    if(!commandArgumentArrayValid)
                    {
                        commandArgumentConstrint.OnInvalidCommandArgumentArray(commandArgumentArray);

                        return false;
                    }
                }

                return true;
            }

            ///// <summary>
            ///// checks whether user entered a wrong number of arguments,
            ///// and logs appropriate message to console in that case. 
            ///// </summary>
            ///// <param name="commandArguments"></param>
            ///// <param name="invalidNumberOfArguments">
            ///// set to true if user entered a wrong number of argument, else false
            ///// </param>
            //protected void HandleWrongNumberOfArguments(
            //    string[] commandArguments,
            //    out bool wrongNumberOfArguments)
            //{
            //    int numberOfArguments = commandArguments.Length;

            //    if (numberOfArguments < minNumberOfArguments || numberOfArguments > maxNumberOfArguments)
            //    {
            //        // wrong number of arguments
            //        wrongNumberOfArguments = true;

            //        ConsoleIOManager.Instance.LogErrorFormat(
            //            false,
            //            "Wrong number of arguments for command: should be between {0} and {1}.",                        
            //            minNumberOfArguments,
            //            maxNumberOfArguments);
            //    }
            //    else // valid number of arguments
            //    {
            //        wrongNumberOfArguments = false;
            //    }
            //}
        }
    }
}