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
            private readonly string prefix;
            
            private bool successfullyExecuted;

            // list of ICommandArgumentConstraints which apply to this command
            protected readonly List<ICommandArgumentConstraint> commandArgumentConstraintList
                = new List<ICommandArgumentConstraint>();

            public Command(string prefix)
            {
                this.prefix = prefix;
                //this.minNumberOfArguments = minNumberOfArguments;
                //this.maxNumberOfArguments = maxNumberOfArguments;
            }

            /// <summary>
            /// unique prefix which determines what command the user requested.
            /// </summary>
            public string Prefix
            {
                get { return prefix; }
            }

            /// <summary>
            /// whether command was successfully executed.
            /// </summary>
            public bool SuccessfullyExecuted
            {
                get { return successfullyExecuted; }
                protected set { successfullyExecuted = value; }
            }

            /// <summary>
            /// handles this command, having specified <paramref name="commandArguments"/>.
            /// </summary>
            /// <param name="commandArguments"></param>
            public void Handle(string[] commandArguments)
            {
                bool commandArgumentsValid = checkCommandArgumentConstraints(commandArguments);

                if(commandArgumentsValid)
                {
                    this.successfullyExecuted = Execute(commandArguments);
                }
            }

            /// <summary>
            /// formats the <see cref="Prefix"/> of a command derived from a base command,
            /// by concatenating <paramref name="baseCommandPrefix"/> and
            /// <paramref name="derivedCommandPrefix"/>.
            /// </summary>
            /// <param name="baseCommandPrefix"></param>
            /// <param name="derivedCommandPrefix"></param>
            /// <returns></returns>
            protected static string FormatPrefix(
                string baseCommandPrefix,
                string derivedCommandPrefix)
            {
                return string.Format("{0} {1}", baseCommandPrefix, derivedCommandPrefix);
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

            /// <summary>
            /// print specified data <paramref name="message"/> to console,
            /// having <see cref="ConsoleIOManager.eOutputReportType"/> of 
            /// <see cref="ConsoleIOManager.eOutputReportType.CommandExecution"/>.
            /// </summary>
            /// <seealso cref="ConsoleIOManager.PrintData(string, ConsoleIOManager.eOutputReportType, bool)"/>
            /// <param name="message"></param>
            /// <param name="flushOutputBuffer"></param>
            protected static void PrintCommandData(string message, bool flushOutputBuffer = false)
            {
                ConsoleIOManager.Instance.PrintData(
                    message,
                    ConsoleIOManager.eOutputReportType.CommandExecution,
                    flushOutputBuffer);
            }

            /// <summary>
            /// prints data with specified <paramref name="format"/>
            /// having attached <paramref name="args"/>,
            /// to console, having <see cref="ConsoleIOManager.eOutputReportType"/> of 
            /// <see cref="ConsoleIOManager.eOutputReportType.CommandExecution"/>.
            /// </summary>
            /// <seealso cref="ConsoleIOManager.PrintDataFormat(bool, ConsoleIOManager.eOutputReportType, string, object[])"/>
            /// <param name="flushOutputBuffer"></param>
            /// <param name="format"></param>
            /// <param name="args"></param>
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

            /// <summary>
            /// logs specified notice <paramref name="message"/> to console,
            /// having <see cref="ConsoleIOManager.eOutputReportType"/> of 
            /// <see cref="ConsoleIOManager.eOutputReportType.CommandExecution"/>. 
            /// </summary>
            /// <seealso cref="ConsoleIOManager.LogNotice(string, ConsoleIOManager.eOutputReportType, bool)"/>
            /// <param name="message"></param>
            /// <param name="flushOutputBuffer"></param>
            protected static void LogCommandNotice(string message, bool flushOutputBuffer = false)
            {
                ConsoleIOManager.Instance.LogNotice(
                    message, 
                    ConsoleIOManager.eOutputReportType.CommandExecution,
                    flushOutputBuffer);
            }

            /// <summary>
            /// logs notice with specified <paramref name="format"/>
            /// having attached <paramref name="args"/>,
            /// to console, having <see cref="ConsoleIOManager.eOutputReportType"/> of 
            /// <see cref="ConsoleIOManager.eOutputReportType.CommandExecution"/>.
            /// </summary>
            /// <seealso cref="ConsoleIOManager.LogNoticeFormat(bool, ConsoleIOManager.eOutputReportType, string, object[])"/>
            /// <param name="flushOutputBuffer"></param>
            /// <param name="format"></param>
            /// <param name="args"></param>
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

            /// <summary>
            /// logs specified error <paramref name="message"/> to console,
            /// having <see cref="ConsoleIOManager.eOutputReportType"/> of 
            /// <see cref="ConsoleIOManager.eOutputReportType.CommandExecution"/>.
            /// </summary>
            /// <seealso cref="ConsoleIOManager.LogError(string, ConsoleIOManager.eOutputReportType, bool)"/>
            /// <param name="message"></param>
            /// <param name="flushOutputBuffer"></param>
            protected static void LogCommandError(string message, bool flushOutputBuffer = false)
            {
                ConsoleIOManager.Instance.LogError(
                    message,
                    ConsoleIOManager.eOutputReportType.CommandExecution,
                    flushOutputBuffer);
            }

            /// <summary>
            /// logs error with specified <paramref name="format"/>,
            /// having attached <paramref name="args"/>,
            /// to console, having <see cref="ConsoleIOManager.eOutputReportType"/> of 
            /// <see cref="ConsoleIOManager.eOutputReportType.CommandExecution"/>.
            /// </summary>
            /// <seealso cref="ConsoleIOManager.LogErrorFormat(bool, ConsoleIOManager.eOutputReportType, string, object[])"/>
            /// <param name="flushOutputBuffer"></param>
            /// <param name="format"></param>
            /// <param name="args"></param>
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

            /// <summary>
            /// logs "refer to error log file" messsage to console, having 
            /// <see cref="ConsoleIOManager.eOutputReportType"/> of
            /// <see cref="ConsoleIOManager.eOutputReportType.CommandExecution"/>.
            /// </summary>
            /// <seealso cref="ExceptionManager.ConsoleLogReferToErrorLogFileMessage(ConsoleIOManager.eOutputReportType, bool)"/>
            /// <param name="flushOutputBuffer"></param>
            protected static void LogCommandReferToErrorLogFileMessage(bool flushOutputBuffer = false)
            {
                ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage(
                    ConsoleIOManager.eOutputReportType.CommandExecution, 
                    flushOutputBuffer);
            }

            /// <summary>
            /// returns whether <paramref name="commandArgumentArray"/> is valid in respect to
            /// <see cref="ICommandArgumentConstraint"/>s associated with this command.
            /// </summary>
            /// <param name="commandArgumentArray"></param>
            /// <returns>
            /// true if <paramref name="commandArgumentArray"/> is valid in respect to
            /// <see cref="ICommandArgumentConstraint"/>s associated with this command,
            /// else false
            /// </returns>
            private bool checkCommandArgumentConstraints(string[] commandArgumentArray)
            {
                foreach(ICommandArgumentConstraint commandArgumentConstrint 
                    in this.commandArgumentConstraintList)
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
        }
    }
}