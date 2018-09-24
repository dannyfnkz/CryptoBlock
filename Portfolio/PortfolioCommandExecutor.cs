using CryptoBlock.CommandHandling;
using CryptoBlock.CommandHandling.Arguments;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using CryptoBlock.PortfolioManagement.Commands;
using CryptoBlock.PortfolioManagement.Commands.TransactionCommands;
using CryptoBlock.PortfolioManagement.Transactions;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using static CryptoBlock.PortfolioManagement.PortfolioManager;
using static CryptoBlock.ServerDataManagement.CoinListingManager;

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        /// <summary>
        /// handles executing portfolio commands.
        /// </summary>
        public class PortfolioCommandExecutor : CommandExecutor
        {          
            private const string COMMAND_TYPE = "Portfolio";

            public PortfolioCommandExecutor()
            {
                // add associations between commands and their prefixes
                AddCommandPrefixToCommandPair(
                    new PortfolioViewCommand(),
                    new PortfolioAddCommand(),
                    new PortfolioRemoveCommand(),
                    new PortfolioClearCommand(),
                    new PortfolioBuyCommand(),
                    new PortfolioSellCommand(),
                    new UndoLastActionCommand()
                    );
            }

            public override string CommandType
            {
                get { return COMMAND_TYPE; }
            }
        }
    }
}