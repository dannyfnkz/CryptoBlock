using CryptoBlock.IOManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace CommandHandling.Arguments
    {
        /// <summary>
        /// represents a <see cref="ICommandArgumentConstraint"/> on the number of arguments passed
        /// to a <see cref="Command"/>.
        /// </summary>
        public class NumberOfArgumentsCommandArgumentConstraint : ICommandArgumentConstraint
        {
            private readonly int minNumberOfArguments;
            private readonly int maxNumberOfArguments;

            public NumberOfArgumentsCommandArgumentConstraint(
                int minNumberOfArguments, 
                int maxNumberOfArguments)
            {
                this.minNumberOfArguments = minNumberOfArguments;
                this.maxNumberOfArguments = maxNumberOfArguments;
            }

            /// <summary>
            /// minimum number of arguments <see cref="Command"/> may receive.
            /// </summary>
            public int MinNumberOfArguments
            {
                get { return minNumberOfArguments; }
            }

            /// <summary>
            /// maximum number of arguments <see cref="Command"/> may receive.
            /// </summary>
            public int MaxNumberOfArguments
            {
                get { return maxNumberOfArguments; }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <seealso cref="ICommandArgumentConstraint.IsValid(string[])"/>
            /// <param name="commandArgumentArray"></param>
            /// <returns></returns>
            bool ICommandArgumentConstraint.IsValid(string[] commandArgumentArray)
            {
                return commandArgumentArray.Length >= minNumberOfArguments
                    && commandArgumentArray.Length <= maxNumberOfArguments;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <seealso cref="ICommandArgumentConstraint.OnInvalidCommandArgumentArray(string[])"/> 
            /// <param name="commandArgumentArray"></param>
            void ICommandArgumentConstraint.OnInvalidCommandArgumentArray(string[] commandArgumentArray)
            {
                ConsoleIOManager.Instance.LogErrorFormat(
                    false,
                    ConsoleIOManager.eOutputReportType.CommandExecution,
                    "Wrong number of arguments for command: should be between {0} and {1}.",
                    minNumberOfArguments,
                    maxNumberOfArguments);
            }
        }
    }
}

