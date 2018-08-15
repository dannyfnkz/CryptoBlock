using CryptoBlock.CommandHandling;
using CryptoBlock.CommandHandling.Arguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace SystemManagement.Commands
    {
        /// <summary>
        /// represents an executable system command.
        /// </summary>
        internal abstract class SystemCommand : Command
        {
            internal SystemCommand(string prefix, int minNumberOfArguments, int maxNumberOfArguments)
                : base(prefix)
            {
                base.commandArgumentConstraintList.Add(
                    new NumberOfArgumentsCommandArgumentConstraint(
                        minNumberOfArguments,
                        maxNumberOfArguments)
                    );
            }
        }
    }

}
