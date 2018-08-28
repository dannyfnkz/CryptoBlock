using CryptoBlock.CommandHandling;
using CryptoBlock.CommandHandling.Arguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace ServerDataManagement.Commands
    {
        /// <summary>
        /// represents an executable server data command.
        /// </summary>
        internal abstract class ServerDataCommand : Command
        {
            private const int MIN_NUMBER_OF_ARGUMENTS = 1;
            private const int MAX_NUMBER_OF_ARGUMENTS = 20;
            
            internal ServerDataCommand(string prefix)
                : base(prefix)
            {
                base.commandArgumentConstraintList.Add(
                    new NumberOfArgumentsCommandArgumentConstraint(
                        MIN_NUMBER_OF_ARGUMENTS,
                        MAX_NUMBER_OF_ARGUMENTS)
                    );
            }
        }
    }
}