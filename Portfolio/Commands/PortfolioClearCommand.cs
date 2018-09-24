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
        /// removes all coins from portfolio.
        /// </summary>
        internal class PortfolioClearCommand : PortfolioCommand
        {
            private const int MIN_NUMBER_OF_ARGUMENTS = 0;
            private const int MAX_NUMBER_OF_ARGUMENTS = 0;

            // command sub-prefix
            private const string SUB_PREFIX = "clear";

            private long[] removedCoinIds;

            internal PortfolioClearCommand()
                : base(SUB_PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
            {

            }

            /// <summary>
            /// removes all <see cref="PortfolioEntry"/>s from portfolio.
            /// returns whether command was executed successfully.
            /// </summary>
            /// <param name="commandArguments"></param>
            /// <returns>
            /// true if command executed successfully,
            /// else false 
            /// </returns>
            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                try
                {
                    // get all coinIds in portfolio
                    long[] coinIdsToBeRemoved = PortfolioManager.Instance.CoinIds;

                    // remove all PortfolioEntry s from portfolio
                    PortfolioManager.Instance.RemoveCoins(coinIdsToBeRemoved);

                    // log successful removal notice to console
                    ConsoleIOManager.Instance.LogNotice(
                        "All entries were successfully removed from portfolio.",
                        ConsoleIOManager.eOutputReportType.CommandExecution);

                    commandExecutedSuccessfuly = true;

                    this.removedCoinIds = coinIdsToBeRemoved;
                }
                catch (DatabaseCommunicationException databaseCommunicationException)
                {
                    PortfolioCommand.HandleDatabaseCommunicationException(databaseCommunicationException);
                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
            }
        }
    }

}
