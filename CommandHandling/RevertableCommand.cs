using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace CommandHandling
    {
        public abstract class RevertableCommand : Command
        {
            public class CommandNotYetExecutedException : Exception
            {
                private readonly string operationName;

                public CommandNotYetExecutedException(string operationName)
                    : base(formatExceptionMessage(operationName))
                {
                    this.operationName = operationName;
                }

                public string OperationName
                {
                    get { return operationName; }
                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "Command must be first executed before performing the following operation: '{0}'.",
                        operationName);
                }
            }

            public RevertableCommand(string prefix)
                : base(prefix)
            {

            }

            public void HandleRevert(string[] commandArguments)
            {
                if(!base.Executed)
                {

                }

                bool revertedSuccessfully = Revert(commandArguments);
                base.Executed = !revertedSuccessfully;
            }

            protected abstract bool Revert(string[] commandArguments);
        }
    }
}