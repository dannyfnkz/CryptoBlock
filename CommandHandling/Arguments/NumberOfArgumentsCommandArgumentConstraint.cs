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
        public class NumberOfArgumentsCommandArgumentConstraint : ICommandArgumentConstraint
        {
            private readonly int minNumberOfArguments;
            private readonly int maxNumberOfArguments;

            public NumberOfArgumentsCommandArgumentConstraint(int minNumberOfArguments, int maxNumberOfArguments)
            {
                this.minNumberOfArguments = minNumberOfArguments;
                this.maxNumberOfArguments = maxNumberOfArguments;
            }

            public int MinNumberOfArguments
            {
                get { return minNumberOfArguments; }
            }

            public int MaxNumberOfArguments
            {
                get { return maxNumberOfArguments; }
            }

            bool ICommandArgumentConstraint.IsValid(string[] commandArgumentArray)
            {
                return commandArgumentArray.Length >= minNumberOfArguments
                    && commandArgumentArray.Length <= maxNumberOfArguments;
            }

            void ICommandArgumentConstraint.OnInvalidCommandArgumentArray(string[] commandArgumentArray)
            {
                ConsoleIOManager.Instance.LogErrorFormat(
                    false,
                    "Wrong number of arguments for command: should be between {0} and {1}.",
                    minNumberOfArguments,
                    maxNumberOfArguments);
            }
        }
    }
}

