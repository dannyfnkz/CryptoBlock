using CryptoBlock.IOManagement;
using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace CommandHandling
    {
        public abstract class Command
        {
            public class InvalidNumberOfArgumentsException : CommandExecutionException
            {
                public InvalidNumberOfArgumentsException(int minNumberOfArguments, int maxNumberOfArguments)
                    : base(formatExceptionMessage(minNumberOfArguments, maxNumberOfArguments))
                {

                }

                private static string formatExceptionMessage(int minNumberOfArguments, int maxNumberOfArguments)
                {
                    return string.Format("Invalid number of arguments: should be between {0} and {1}.",
                        minNumberOfArguments,
                        maxNumberOfArguments);
                }
            }

            private readonly string prefix;
            private readonly int minNumberOfArguments;
            private readonly int maxNumberOfArguments;

            public Command(string prefix, int minNumberOfArguments, int maxNumberOfArguments)
            {
                this.prefix = prefix;
                this.minNumberOfArguments = minNumberOfArguments;
                this.maxNumberOfArguments = maxNumberOfArguments;
            }

            public string Prefix
            {
                get { return prefix; }
            }

            public int MinNumberOfArguments
            {
                get { return minNumberOfArguments; }
            }

            public int MaxNumberOfArguments
            {
                get { return maxNumberOfArguments; }
            }

            public abstract void ExecuteCommand(string[] commandArguments);

            protected void HandleInvalidNumberOfArguments(
                string[] commandArguments,
                out bool invalidNumberOfArguments)
            {
                int numberOfArguments = commandArguments.Length;

                if (numberOfArguments < minNumberOfArguments || numberOfArguments > maxNumberOfArguments)
                {
                    // invalid number of arguments
                    invalidNumberOfArguments = true;

                    ConsoleIOManager.Instance.LogErrorFormat(
                        false,
                        "Invalid number of arguments for command: should be between {0} and {1}.",                        
                        minNumberOfArguments,
                        maxNumberOfArguments);
                }
                else // valid number of arguments
                {
                    invalidNumberOfArguments = false;
                }
            }
        }
    }
}