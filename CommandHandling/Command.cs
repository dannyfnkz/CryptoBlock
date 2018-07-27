using CryptoBlock.IOManagement;

namespace CryptoBlock
{
    namespace CommandHandling
    {
        /// <summary>
        /// represents a single executable command.
        /// </summary>
        public abstract class Command
        {
            /// <summary>
            /// thrown if user gave a wrong number of arguments for command. 
            /// </summary>
            public class WrongNumberOfArgumentsException : CommandExecutionException
            {
                public WrongNumberOfArgumentsException(int minNumberOfArguments, int maxNumberOfArguments)
                    : base(formatExceptionMessage(minNumberOfArguments, maxNumberOfArguments))
                {

                }

                private static string formatExceptionMessage(int minNumberOfArguments, int maxNumberOfArguments)
                {
                    return string.Format("wrong number of arguments: should be between {0} and {1}.",
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

            /// <summary>
            /// unique prefix determines what command the user requested.
            /// </summary>
            public string Prefix
            {
                get { return prefix; }
            }

            /// <summary>
            /// minimum number of arguments allowed for command.
            /// </summary>
            public int MinNumberOfArguments
            {
                get { return minNumberOfArguments; }
            }

            /// <summary>
            /// maximum number of arguments allowed for command.
            /// </summary>
            public int MaxNumberOfArguments
            {
                get { return maxNumberOfArguments; }
            }

            /// <summary>
            /// executes command with given <paramref name="commandArguments"/>.
            /// </summary>
            /// <param name="commandArguments"></param>
            public abstract void ExecuteCommand(string[] commandArguments);

            /// <summary>
            /// checks whether user entered a wrong number of arguments,
            /// and logs appropriate message to console in that case. 
            /// </summary>
            /// <param name="commandArguments"></param>
            /// <param name="invalidNumberOfArguments">
            /// set to true if user entered a wrong number of argument, else false
            /// </param>
            protected void HandleWrongNumberOfArguments(
                string[] commandArguments,
                out bool wrongNumberOfArguments)
            {
                int numberOfArguments = commandArguments.Length;

                if (numberOfArguments < minNumberOfArguments || numberOfArguments > maxNumberOfArguments)
                {
                    // wrong number of arguments
                    wrongNumberOfArguments = true;

                    ConsoleIOManager.Instance.LogErrorFormat(
                        false,
                        "Wrong number of arguments for command: should be between {0} and {1}.",                        
                        minNumberOfArguments,
                        maxNumberOfArguments);
                }
                else // valid number of arguments
                {
                    wrongNumberOfArguments = false;
                }
            }
        }
    }
}