using CryptoBlock.CommandHandling;
using CryptoBlock.CommandHandling.Arguments;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.PortfolioManagement.PortfolioManager;

namespace CryptoBlock
{
    namespace PortfolioManagement.Commands
    {
        /// <summary>
        /// represents an executable portfolio command.
        /// </summary>
        internal abstract class PortfolioCommand : Command
        {
            internal PortfolioCommand(
                string inheritingCommandPrefix,
                int minNumberOfArguments,
                int maxNumberOfArguments)
                : base(PortfolioCommandUtils.FormatPrefix(inheritingCommandPrefix))
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